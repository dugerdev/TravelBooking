using System.Linq.Expressions;
using TravelBooking.Application.Abstractions.Persistence;
using TravelBooking.Application.Common;
using TravelBooking.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace TravelBooking.Infrastructure.Repositories;

//---Entity Framework Core tabanli generic repository implementasyonu---//
//---Tum entity tiplerinin ortak CRUD davranisini burada topluyoruz---//
public class EfRepositoryBase<T> : IRepository<T> where T : BaseEntity
{
    private readonly DbContext _context;                                              //---SaveChanges / transaction gibi islemler icin gerekiyor---//
    protected readonly DbSet<T> _dbSet;                                               //---EF Core'un ilgili tabloya erisim noktasi---//

    public EfRepositoryBase(DbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();                                                    //---EF Core entity tipine gore dogru DbSet'i verir---//
    }

    //---Primary key'e gore tek kayit donduren metot---//
    //---Soft delete edilmis olanlari disarida tutar---//
    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _dbSet.FirstOrDefaultAsync(
            entity => entity.Id == id && !entity.IsDeleted,
            cancellationToken);

    //---Primary key'e gore tek kayit donduren metot (Include destegi ile)---//
    //---Navigation property'leri de yukler---//
    public async Task<T?> GetByIdWithIncludesAsync(Guid id, CancellationToken cancellationToken = default, params Expression<Func<T, object>>[] includes)
    {
        var query = _dbSet.Where(entity => entity.Id == id && !entity.IsDeleted).AsQueryable();
        
        // Include'lari uygula
        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        return await query.FirstOrDefaultAsync(cancellationToken);
    }
    
    //---ThenInclude destegi icin protected metod (gelecekte kullanilabilir)---//

    //---Tum kayitlari (IsDeleted = false) liste olarak donduren metot---//
    public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _dbSet.Where(entity => !entity.IsDeleted)
                       .ToListAsync(cancellationToken);

    //---Tum kayitlari pagination ile donduren metot---//
    public async Task<PagedResult<T>> GetAllPagedAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        var pageNumber = request.GetValidPageNumber();
        var pageSize = request.GetValidPageSize();

        var query = _dbSet.Where(entity => !entity.IsDeleted);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(x => x.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<T>(items, totalCount, pageNumber, pageSize);
    }

    //---Verilen kosula gore filtrelenmis kayit listesini donduren metot---//
    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        => await _dbSet.Where(entity => !entity.IsDeleted)
                       .Where(predicate)
                       .ToListAsync(cancellationToken);

    //---Verilen kosula gore filtrelenmis kayitlari pagination ile donduren metot---//
    public async Task<PagedResult<T>> FindPagedAsync(Expression<Func<T, bool>> predicate, PagedRequest request, CancellationToken cancellationToken = default)
    {
        var pageNumber = request.GetValidPageNumber();
        var pageSize = request.GetValidPageSize();

        var query = _dbSet.Where(entity => !entity.IsDeleted)
                          .Where(predicate);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(x => x.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<T>(items, totalCount, pageNumber, pageSize);
    }

    //---Yeni bir kayit ekleyen metot---//
    //---Kalici olmasi icin UnitOfWork.SaveChangesAsync cagrilmalidir---//
    public async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
        return entity;
    }

    //---Birden fazla kaydi toplu ekleyen metot---//
    public async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        => await _dbSet.AddRangeAsync(entities, cancellationToken);

    //---Mevcut kaydi guncelleyen metot---//
    //---SaveChanges ile kalici olur---//
    public Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Update(entity);
        return Task.CompletedTask;
    }

    //---Fiziksel silme operasyonu---//
    //---Soft delete icin asagidaki metodu kullan---//
    public Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Remove(entity);
        return Task.CompletedTask;
    }

    //---Birden fazla kaydi toplu silen metot---//
    public Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        _dbSet.RemoveRange(entities);
        return Task.CompletedTask;
    }

    //---Kayit soft delete eden metot---//
    //---IsDeleted, IsActive alanlari guncellenir---//
    public async Task SoftDeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        //---GetByIdAsync IsDeleted filtresi yapar, bu yuzden direkt DbSet'ten aliyoruz---//
        var entity = await _dbSet.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        if (entity is null) return;                                                   //---Kayit bulunmazsa islem yapilmaz---//
        
        //---Zaten silinmisse tekrar silme---//
        if (entity.IsDeleted) return;

        entity.IsDeleted = true;                                                      //---Soft delete isaretini ata---//
        entity.IsActive = false;                                                      //---Artik aktif degil---//

        _dbSet.Update(entity);                                                        //---ChangeTracker degisikligi gozlemler---//
    }

    //---Belirtilen id'ye sahip (soft delete edilmemis) kayit var mi kontrol eden metot---//
    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        => await _dbSet.AnyAsync(entity => entity.Id == id && !entity.IsDeleted, cancellationToken);

    //---IQueryable donduren metot (Include ve custom query'ler icin)---//
    public IQueryable<T> GetQueryable()
        => _dbSet.Where(entity => !entity.IsDeleted).AsQueryable();
}

