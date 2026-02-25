using TravelBooking.Application.Abstractions.Persistence;
using TravelBooking.Domain.Common;
using TravelBooking.Domain.Entities;
using TravelBooking.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using TravelBooking.Infrastructure.Data;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Data.SqlClient;

namespace TravelBooking.Infrastructure.Repositories;

//---Repository orneklerini tek merkezden yoneten ve transaction / SaveChanges yonetimini ustlenen sinif---//
public class UnitOfWork(TravelBookingDbContext context, IDomainEventDispatcher domainEventDispatcher, ILogger<UnitOfWork>? logger = null) : IUnitOfWork
{
    private readonly TravelBookingDbContext _context = context;                                       //---EF Core context'i (TravelBookingDbContext enjekte edilecek)---//
    private readonly IDomainEventDispatcher _domainEventDispatcher = domainEventDispatcher;                  //---Domain event dispatcher---//
    private readonly ILogger<UnitOfWork>? _logger = logger;                                      //---Logger instance---//
    private IDbContextTransaction? _transaction;                                      //---Aktif transaction referansi (varsa)---//
    private readonly Dictionary<Type, object> _repositories = [];                //---Repository orneklerini cache'lemek icin---//

    //---Her property ilk cagrildiginda EfRepositoryBase<T> ornegi olusturup cache'e ekler---//
    public IRepository<Flight> Flights => GetRepository<Flight>();
    public IRepository<Reservation> Reservations => GetRepository<Reservation>();
    public IRepository<Ticket> Tickets => GetRepository<Ticket>();
    public IRepository<Passenger> Passengers => GetRepository<Passenger>();
    public IRepository<Payment> Payments => GetRepository<Payment>();
    public IRepository<Airport> Airports => GetRepository<Airport>();
    public IRepository<Hotel> Hotels => GetRepository<Hotel>();
    public IRepository<Room> Rooms => GetRepository<Room>();
    public IRepository<Car> Cars => GetRepository<Car>();
    public IRepository<Tour> Tours => GetRepository<Tour>();
    public IRepository<NewsArticle> News => GetRepository<NewsArticle>();
    public IRepository<ContactMessage> ContactMessages => GetRepository<ContactMessage>();
    public IRepository<Testimonial> Testimonials => GetRepository<Testimonial>();

    // ThenInclude ve complex query'ler icin DbContext erisimi
    public DbContext Context => _context;

    //---EF Core'un SaveChangesAsync metodunu sarar---//
    //---Yapilan tum degisiklikleri kalici hale getirir---//
    //---Domain event'leri de dispatch eder---//
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Entity'lerden domain event'leri topla
        var domainEvents = GetDomainEvents();

        try
        {
            // Track edilen entity'leri logla (debug icin)
            if (_logger?.IsEnabled(LogLevel.Debug) == true)
            {
                var entries = _context.ChangeTracker.Entries()
                    .Where(e => e.State != EntityState.Unchanged)
                    .ToList();
                
                if (entries.Count > 0)
                {
                    _logger.LogDebug("Saving changes for {Count} entities", entries.Count);
                    foreach (var entry in entries)
                    {
                        _logger.LogDebug("Entity: {EntityType}, State: {State}", 
                            entry.Entity.GetType().Name, entry.State);
                    }
                }
            }

            // Once SaveChanges'i yap
            var result = await _context.SaveChangesAsync(cancellationToken);

            // Basarili olursa domain event'leri dispatch et
            if (result > 0 && domainEvents.Count > 0)
            {
                await _domainEventDispatcher.DispatchAsync(domainEvents, cancellationToken);
                
                // Event'leri temizle
                ClearDomainEvents();
            }

            return result;
        }
        catch (DbUpdateException dbEx)
        {
            if (_transaction is not null)
            {
                await RollbackTransactionAsync(cancellationToken);
            }

            // Detayli hata bilgisi topla
            var errorMessage = ExtractDetailedErrorMessage(dbEx);
            
            // Inner exception'dan daha fazla bilgi al
            var innerMessage = dbEx.InnerException?.Message ?? "Inner exception yok";
            var fullStackTrace = dbEx.InnerException?.StackTrace ?? dbEx.StackTrace ?? "Stack trace yok";
            
            _logger?.LogError(dbEx, "Database update error: {ErrorMessage}. Inner: {InnerMessage}", 
                errorMessage, innerMessage);
            
            // Inner exception varsa onu da logla
            if (dbEx.InnerException != null)
            {
                _logger?.LogError(dbEx.InnerException, "Inner exception details: {InnerMessage}. StackTrace: {StackTrace}", 
                    dbEx.InnerException.Message, fullStackTrace);
                
                // SQL Server hata kodlarini logla
                if (dbEx.InnerException is SqlException sqlEx)
                {
                    _logger?.LogError("SQL Error Number: {ErrorNumber}, Line: {LineNumber}, Procedure: {Procedure}", 
                        sqlEx.Number, sqlEx.LineNumber, sqlEx.Procedure);
                }
            }

            // Track edilen entity'leri logla
            var entries = _context.ChangeTracker.Entries()
                .Where(e => e.State != EntityState.Unchanged)
                .ToList();
            
            foreach (var entry in entries)
            {
                _logger?.LogError("Failed entity: {EntityType}, State: {State}, Entity: {@Entity}", 
                    entry.Entity.GetType().Name, entry.State, entry.Entity);
            }

            // Inner exception mesajini da iceren daha detayli hata mesaji olustur
            var detailedMessage = $"{errorMessage}. Inner Exception: {innerMessage}";
            throw new DbUpdateException(detailedMessage, dbEx);
        }
    }

    //---DbUpdateException'dan detayli hata mesaji cikaran metot---//
    private static string ExtractDetailedErrorMessage(DbUpdateException dbEx)
    {
        var message = dbEx.Message;

        // Inner exception mesajini ekle
        if (dbEx.InnerException != null)
        {
            message += $" Inner Exception: {dbEx.InnerException.Message}";
        }

        // SQL Server hatalari icin ozel mesajlar
        // .NET 9'da Microsoft.Data.SqlClient.SqlException kullanilir
        if (dbEx.InnerException is SqlException sqlEx)
        {
            // Foreign key constraint violation
            if (sqlEx.Number == 547)
            {
                var constraintName = ExtractConstraintName(sqlEx.Message);
                message = $"Foreign key constraint violation ({constraintName}). " +
                         "The operation cannot be completed because it would violate a foreign key constraint. " +
                         "Please ensure all referenced entities exist and are not deleted.";
            }
            // Unique constraint violation
            else if (sqlEx.Number == 2627 || sqlEx.Number == 2601)
            {
                var constraintName = ExtractConstraintName(sqlEx.Message);
                message = $"Unique constraint violation ({constraintName}). " +
                         "A duplicate value was found. Please ensure unique values.";
            }
            // Not null constraint violation
            else if (sqlEx.Number == 515)
            {
                message = "Cannot insert NULL value. A required field is missing.";
            }
        }

        return message;
    }

    //---Hata mesajindan constraint adini cikaran metot---//
    private static string ExtractConstraintName(string errorMessage)
    {
        // Constraint adini bulmak icin regex veya string isleme
        // Ornek: "FK_Payments_Reservations_ReservationId" gibi
        // Note: Using compiled regex for better performance
        const string pattern = @"(?:constraint|CONSTRAINT|key|KEY)\s+['""]?([^'""\s]+)['""]?";
        var constraintMatch = System.Text.RegularExpressions.Regex.Match(
            errorMessage, 
            pattern,
            System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Compiled);
        
        return constraintMatch.Success ? constraintMatch.Groups[1].Value : "Unknown";
    }

    //---Track edilen entity'lerden domain event'leri toplayan metot---//
    private List<IDomainEvent> GetDomainEvents()
    {
        var domainEvents = new List<IDomainEvent>();

        var entities = _context.ChangeTracker
            .Entries<BaseEntity>()
            .Where(e => e.Entity.DomainEvents.Count > 0)
            .Select(e => e.Entity)
            .ToList();

        foreach (var entity in entities)
        {
            domainEvents.AddRange(entity.DomainEvents);
        }

        return domainEvents;
    }

    //---Track edilen entity'lerden domain event'leri temizleyen metot---//
    private void ClearDomainEvents()
    {
        var entities = _context.ChangeTracker
            .Entries<BaseEntity>()
            .Where(e => e.Entity.DomainEvents.Count > 0)
            .Select(e => e.Entity)
            .ToList();

        foreach (var entity in entities)
        {
            entity.ClearDomainEvents();
        }
    }

    //---Transaction baslatan metot---//
    //---Zaten aktif bir transaction varsa yeni bir tane acilmaz---//
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is not null) return;
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    //---Transaction baslatan metot (isolation level ile)---//
    public async Task BeginTransactionAsync(System.Data.IsolationLevel isolationLevel, CancellationToken cancellationToken = default)
    {
        if (_transaction is not null) return;
        _transaction = await _context.Database.BeginTransactionAsync(isolationLevel, cancellationToken);
    }

    //---Transaction'i commit eden metot---//
    //---Kaynagi serbest birakir---//
    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is null) return;

        await _transaction.CommitAsync(cancellationToken);
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    //---Transaction'i rollback eden metot---//
    //---Herhangi bir hata durumunda geri donus saglar---//
    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is null) return;

        await _transaction.RollbackAsync(cancellationToken);
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    //---UnitOfWork yasam dongusu bittiginde transaction ve DbContext'i temizler---//
    public async ValueTask DisposeAsync()
    {
        if (_transaction is not null)
            await _transaction.DisposeAsync();

        await _context.DisposeAsync();
        GC.SuppressFinalize(this);
    }

    //---Generic repository ornegini lazily olusturur ve sozlukte cache'ler---//
    //---Boylece ayni entity icin tekrar tekrar nesne uretilmez---//
    private IRepository<TEntity> GetRepository<TEntity>() where TEntity : BaseEntity
    {
        var type = typeof(TEntity);

        if (!_repositories.TryGetValue(type, out var repository))
        {
            repository = new EfRepositoryBase<TEntity>(_context);                      //---Ilk defa isteniyorsa olustur---//
            _repositories[type] = repository;                                         //---Sozluge ekle---//
        }

        return (IRepository<TEntity>)repository;
    }
}

