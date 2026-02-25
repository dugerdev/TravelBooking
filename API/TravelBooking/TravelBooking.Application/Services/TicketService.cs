using TravelBooking.Application.Contracts;
using TravelBooking.Application.Common;
using TravelBooking.Application.Abstractions.Persistence;
using TravelBooking.Domain.Entities;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace TravelBooking.Application.Services;

/// <summary>
/// Biletlere iliskin is kurallarini yoneten servis.
/// Bilet olusturma, guncelleme, iptal etme ve sorgulama islemlerini yonetir.
/// </summary>
public class TicketService : ITicketService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TicketService> _logger;

    /// <summary>
    /// TicketService constructor.
    /// </summary>
    /// <param name="unitOfWork">Unit of Work instance.</param>
    /// <param name="logger">Logger instance.</param>
    public TicketService(IUnitOfWork unitOfWork, ILogger<TicketService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// ID'ye gore bilet getirir. Reservation, Flight ve Passenger navigation property'lerini de yukler.
    /// </summary>
    /// <param name="id">Bilet ID'si.</param>
    /// <param name="cancellationToken">Iptal token'i.</param>
    /// <returns>Bilet bilgileri veya hata mesaji.</returns>
    public async Task<DataResult<Ticket>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Include navigation properties to avoid N+1 queries
        var ticket = await _unitOfWork.Context.Set<Ticket>()
            .Where(t => t.Id == id && !t.IsDeleted)
            .Include(t => t.Reservation)
            .Include(t => t.Flight)
            .Include(t => t.Passenger)
            .FirstOrDefaultAsync(cancellationToken);

        if (ticket is null)
        {
            _logger.LogWarning("Ticket not found with id: {TicketId}", id);
            return new ErrorDataResult<Ticket>(null!, "Bilet bulunamadi.");
        }

        return new SuccessDataResult<Ticket>(ticket);
    }

    //---Rezervasyon ID'sine gore biletleri getiren metot---//
    public async Task<DataResult<IEnumerable<Ticket>>> GetByReservationIdAsync(Guid reservationId, CancellationToken cancellationToken = default)
    {
        // Include navigation properties to avoid N+1 queries
        var tickets = await _unitOfWork.Context.Set<Ticket>()
            .Where(t => t.ReservationId == reservationId && !t.IsDeleted)
            .Include(t => t.Flight)
            .Include(t => t.Passenger)
            .ToListAsync(cancellationToken);
        return new SuccessDataResult<IEnumerable<Ticket>>(tickets);
    }

    //---Ucus ID'sine gore biletleri getiren metot---//
    public async Task<DataResult<IEnumerable<Ticket>>> GetByFlightIdAsync(Guid flightId, CancellationToken cancellationToken = default)
    {
        var query = _unitOfWork.Tickets.GetQueryable()
            .Include(t => t.Flight)
            .Include(t => t.Passenger)
            .Include(t => t.Reservation)
            .Where(t => t.FlightId == flightId);

        var tickets = await query.ToListAsync(cancellationToken);
        return new SuccessDataResult<IEnumerable<Ticket>>(tickets);
    }

    //---Yolcu ID'sine gore biletleri getiren metot---//
    public async Task<DataResult<IEnumerable<Ticket>>> GetByPassengerIdAsync(Guid passengerId, CancellationToken cancellationToken = default)
    {
        var query = _unitOfWork.Tickets.GetQueryable()
            .Include(t => t.Flight)
            .Include(t => t.Passenger)
            .Include(t => t.Reservation)
            .Where(t => t.PassengerId == passengerId);

        var tickets = await query.ToListAsync(cancellationToken);
        return new SuccessDataResult<IEnumerable<Ticket>>(tickets);
    }

    //---Tum biletleri getiren metot---//
    public async Task<DataResult<IEnumerable<Ticket>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var tickets = await _unitOfWork.Tickets.GetAllAsync(cancellationToken);
        return new SuccessDataResult<IEnumerable<Ticket>>(tickets);
    }

    //---Tum biletleri pagination ile getiren metot---//
    public async Task<DataResult<PagedResult<Ticket>>> GetAllPagedAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting tickets with pagination: Page {PageNumber}, Size {PageSize}", request.PageNumber, request.PageSize);
        
        var pagedResult = await _unitOfWork.Tickets.GetAllPagedAsync(request, cancellationToken);
        return new SuccessDataResult<PagedResult<Ticket>>(pagedResult);
    }

    //---Yeni bilet ekleyen metot---//
    public async Task<Result> AddAsync(Ticket ticket, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Adding new ticket: FlightId {FlightId}, ReservationId {ReservationId}", ticket.FlightId, ticket.ReservationId);
        
        try
        {
            await _unitOfWork.Tickets.AddAsync(ticket, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Ticket added successfully: {TicketId}", ticket.Id);
            return new SuccessResult("Bilet eklendi.");
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
        {
            var innerMessage = dbEx.InnerException?.Message ?? "Bilinmeyen hata";
            _logger.LogError(dbEx, "Database error while adding ticket: FlightId {FlightId}, ReservationId {ReservationId}. Inner: {InnerMessage}", 
                ticket.FlightId, ticket.ReservationId, innerMessage);
            
            if (dbEx.InnerException is Microsoft.Data.SqlClient.SqlException sqlEx)
            {
                if (sqlEx.Number == 547) // Foreign key constraint
                    return new ErrorResult($"Bilet eklenirken hata: Iliskili kayit bulunamadi (Ucus, Rezervasyon veya Yolcu). SQL: {sqlEx.Message}");
                else if (sqlEx.Number == 515)
                    return new ErrorResult($"Bilet eklenirken hata: Zorunlu alan eksik. SQL: {sqlEx.Message}");
            }
            
            return new ErrorResult($"Bilet eklenirken veritabani hatasi: {innerMessage}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding ticket: FlightId {FlightId}, ReservationId {ReservationId}", ticket.FlightId, ticket.ReservationId);
            return new ErrorResult($"Bilet eklenirken hata olustu: {ex.Message}");
        }
    }

    //---Birden fazla bilet ekleyen metot---//
    public async Task<Result> AddRangeAsync(IEnumerable<Ticket> tickets, CancellationToken cancellationToken = default)
    {
        var ticketList = tickets.ToList();
        _logger.LogInformation("Adding {Count} tickets", ticketList.Count);

        try
        {
            await _unitOfWork.Tickets.AddRangeAsync(ticketList, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new SuccessResult($"{ticketList.Count} bilet eklendi.");
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
        {
            var innerMessage = dbEx.InnerException?.Message ?? "Bilinmeyen hata";
            _logger.LogError(dbEx, "Database error while adding {Count} tickets. Inner: {InnerMessage}", ticketList.Count, innerMessage);
            
            if (dbEx.InnerException is Microsoft.Data.SqlClient.SqlException sqlEx && sqlEx.Number == 547)
                return new ErrorResult($"Biletler eklenirken hata: Iliskili kayit bulunamadi. SQL: {sqlEx.Message}");
            
            return new ErrorResult($"Biletler eklenirken veritabani hatasi: {innerMessage}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding {Count} tickets", ticketList.Count);
            return new ErrorResult($"Biletler eklenirken hata olustu: {ex.Message}");
        }
    }

    //---Mevcut bileti guncelleyen metot---//
    public async Task<Result> UpdateAsync(Ticket ticket, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating ticket: {TicketId}", ticket.Id);
        
        await _unitOfWork.Tickets.UpdateAsync(ticket, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Ticket updated successfully: {TicketId}", ticket.Id);
        return new SuccessResult("Bilet guncellendi.");
    }

    //---Bileti iptal eden metot---//
    public async Task<Result> CancelTicketAsync(Guid ticketId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Cancelling ticket: {TicketId}", ticketId);

        var ticket = await _unitOfWork.Tickets.GetByIdAsync(ticketId, cancellationToken);
        if (ticket == null)
            return new ErrorResult("Bilet bulunamadi.");

        if (ticket.TicketStatus == Domain.Enums.TicketStatus.Cancelled)
            return new ErrorResult("Bilet zaten iptal edilmis.");

        try
        {
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            //---Bilet durumunu iptal olarak guncelle---//
            ticket.UpdateStatus(Domain.Enums.TicketStatus.Cancelled);

            //---Ucustan koltuklari serbest birak---//
            var flight = await _unitOfWork.Flights.GetByIdAsync(ticket.FlightId, cancellationToken);
            if (flight != null)
            {
                //---Bilet basina 1 koltuk serbest birak (her bilet 1 koltuk icin)---//
                flight.ReleaseSeats(1);
                await _unitOfWork.Flights.UpdateAsync(flight, cancellationToken);
            }

            await _unitOfWork.Tickets.UpdateAsync(ticket, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("Ticket cancelled successfully: {TicketId}", ticketId);
            return new SuccessResult("Bilet iptal edildi.");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Error cancelling ticket: {TicketId}", ticketId);
            return new ErrorResult($"Bilet iptal edilirken hata olustu: {ex.Message}");
        }
    }

    //---Bilete koltuk numarasi atayan metot---//
    public async Task<Result> AssignSeatAsync(Guid ticketId, string seatNumber, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Assigning seat {SeatNumber} to ticket: {TicketId}", seatNumber, ticketId);

        var ticket = await _unitOfWork.Tickets.GetByIdAsync(ticketId, cancellationToken);
        if (ticket == null)
            return new ErrorResult("Bilet bulunamadi.");

        ticket.AssignSeat(seatNumber);
        await _unitOfWork.Tickets.UpdateAsync(ticket, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Seat assigned successfully: TicketId {TicketId}, SeatNumber {SeatNumber}", ticketId, seatNumber);
        return new SuccessResult("Koltuk numarasi atandi.");
    }
}
