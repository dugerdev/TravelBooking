using TravelBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace TravelBooking.Application.Abstractions.Persistence;

public interface IUnitOfWork : IAsyncDisposable
{
    IRepository<Flight> Flights { get; }
    IRepository<Reservation> Reservations { get; }
    IRepository<Ticket> Tickets { get; }
    IRepository<Passenger> Passengers { get; }
    IRepository<Payment> Payments { get; }
    IRepository<Airport> Airports { get; }
    IRepository<Hotel> Hotels { get; }
    IRepository<Room> Rooms { get; }
    IRepository<Car> Cars { get; }
    IRepository<Tour> Tours { get; }
    IRepository<NewsArticle> News { get; }
    IRepository<ContactMessage> ContactMessages { get; }
    IRepository<Testimonial> Testimonials { get; }

    // ThenInclude ve complex query'ler icin DbContext erisimi
    DbContext Context { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
