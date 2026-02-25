using TravelBooking.Application.Common;
using TravelBooking.Domain.Entities;

namespace TravelBooking.Application.Contracts;

public interface IContactMessageService
{
    Task<DataResult<ContactMessage>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<DataResult<IEnumerable<ContactMessage>>> GetAllAsync(string? statusFilter = null, string? searchQuery = null, CancellationToken cancellationToken = default);
    Task<Result> AddAsync(ContactMessage contactMessage, CancellationToken cancellationToken = default);
    Task<Result> MarkAsReadAsync(Guid id, string readBy, CancellationToken cancellationToken = default);
    Task<Result> AddResponseAsync(Guid id, string response, string responseBy, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<int> GetUnreadCountAsync(CancellationToken cancellationToken = default);
}
