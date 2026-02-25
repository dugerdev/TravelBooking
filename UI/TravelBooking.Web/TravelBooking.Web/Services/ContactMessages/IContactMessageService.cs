using TravelBooking.Web.Models;

namespace TravelBooking.Web.Services.ContactMessages;

public interface IContactMessageService
{
    Task<(bool success, string message)> CreateAsync(ContactMessage contactMessage, CancellationToken ct = default);
    Task<(bool success, string message, List<ContactMessage>? messages)> GetAllAsync(string? statusFilter = null, string? searchQuery = null, CancellationToken ct = default);
    Task<(bool success, string message, ContactMessage? contactMessage)> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<(bool success, string message)> MarkAsReadAsync(Guid id, string readBy, CancellationToken ct = default);
    Task<(bool success, string message)> AddResponseAsync(Guid id, string response, string responseBy, CancellationToken ct = default);
    Task<(bool success, string message)> DeleteAsync(Guid id, CancellationToken ct = default);
    Task<int> GetUnreadCountAsync(CancellationToken ct = default);
}
