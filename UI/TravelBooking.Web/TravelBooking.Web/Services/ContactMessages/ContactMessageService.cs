using TravelBooking.Web.Constants;
using TravelBooking.Web.Models;
using TravelBooking.Web.DTOs.ContactMessages;
using TravelBooking.Web.DTOs.Common;
using TravelBooking.Web.Services.TravelBookingApi;

namespace TravelBooking.Web.Services.ContactMessages;

/// <summary>
/// Contact message service that persists via the backend API.
/// </summary>
public class ContactMessageService : IContactMessageService
{
    private readonly ITravelBookingApiClient _api;
    private const string BasePath = "api/ContactMessages";

    public ContactMessageService(ITravelBookingApiClient api)
    {
        _api = api;
    }

    public async Task<(bool success, string message)> CreateAsync(ContactMessage contactMessage, CancellationToken ct = default)
    {
        var dto = new CreateContactMessageDto
        {
            Name = contactMessage.Name,
            Email = contactMessage.Email,
            Phone = contactMessage.Phone ?? string.Empty,
            Subject = contactMessage.Subject ?? "General Inquiry",
            Message = contactMessage.Message
        };
        var res = await _api.PostAsync<ContactMessageDto>(ApiEndpoints.ContactMessages, dto, ct);
        if (res == null)
            return (false, "Unable to submit message. Please try again.");
        if (!res.Success)
            return (false, res.Message ?? "An error occurred.");
        return (true, res.Message ?? "Contact message submitted successfully.");
    }

    public async Task<(bool success, string message, List<ContactMessage>? messages)> GetAllAsync(
        string? statusFilter = null,
        string? searchQuery = null,
        CancellationToken ct = default)
    {
        var query = new List<string>();
        if (!string.IsNullOrEmpty(statusFilter))
            query.Add($"statusFilter={Uri.EscapeDataString(statusFilter)}");
        if (!string.IsNullOrEmpty(searchQuery))
            query.Add($"searchQuery={Uri.EscapeDataString(searchQuery)}");
        var path = query.Count > 0 ? $"{BasePath}?{string.Join("&", query)}" : BasePath;

        var res = await _api.GetAsync<List<ContactMessageDto>>(path, ct);
        if (res == null || !res.Success)
            return (false, res?.Message ?? "Failed to load messages.", null);
        var list = res.Data?.Select(ToModel).ToList() ?? new List<ContactMessage>();
        return (true, res.Message ?? "Messages retrieved successfully.", list);
    }

    public async Task<(bool success, string message, ContactMessage? contactMessage)> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var res = await _api.GetAsync<ContactMessageDto>(ApiEndpoints.ContactMessageById(id), ct);
        if (res == null || !res.Success)
            return (false, res?.Message ?? "Message not found.", null);
        var model = res.Data != null ? ToModel(res.Data) : null;
        return (true, res.Message ?? "Message retrieved successfully.", model);
    }

    public async Task<(bool success, string message)> MarkAsReadAsync(Guid id, string readBy, CancellationToken ct = default)
    {
        var res = await _api.PostAsync<object>(ApiEndpoints.ContactMessageMarkRead(id), new { ReadBy = readBy }, ct);
        if (res == null)
            return (false, "Request failed.");
        if (!res.Success)
            return (false, res.Message ?? "Failed to mark as read.");
        return (true, res.Message ?? "Message marked as read.");
    }

    public async Task<(bool success, string message)> AddResponseAsync(Guid id, string response, string responseBy, CancellationToken ct = default)
    {
        var body = new { Response = response, ResponseBy = responseBy };
        var res = await _api.PostAsync<object>(ApiEndpoints.ContactMessageResponse(id), body, ct);
        if (res == null)
            return (false, "Request failed.");
        if (!res.Success)
            return (false, res.Message ?? "Failed to add response.");
        return (true, res.Message ?? "Response added successfully.");
    }

    public async Task<(bool success, string message)> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var res = await _api.DeleteAsync<object>($"{BasePath}/{id}", ct);
        if (res == null)
            return (false, "Request failed.");
        if (!res.Success)
            return (false, res.Message ?? "Failed to delete.");
        return (true, res.Message ?? "Message deleted successfully.");
    }

    public async Task<int> GetUnreadCountAsync(CancellationToken ct = default)
    {
        // API returns ApiResult<object> with Data as integer, we need to handle it properly
        try
        {
            var res = await _api.GetAsync<object>(ApiEndpoints.ContactMessagesUnreadCount, ct);
            if (res == null || !res.Success || res.Data == null)
                return 0;
            
            // Try to convert the Data to int
            if (res.Data is int intValue)
                return intValue;
            if (int.TryParse(res.Data.ToString(), out var parsedValue))
                return parsedValue;
            
            return 0;
        }
        catch
        {
            return 0;
        }
    }

    private static ContactMessage ToModel(ContactMessageDto dto)
    {
        return new ContactMessage
        {
            Id = dto.Id,
            Name = dto.Name,
            Email = dto.Email,
            Phone = dto.Phone ?? string.Empty,
            Subject = dto.Subject,
            Message = dto.Message,
            IsRead = dto.IsRead,
            CreatedDate = dto.CreatedDate,
            ReadDate = dto.ReadDate,
            ReadBy = dto.ReadBy,
            Response = dto.Response,
            ResponseDate = dto.ResponseDate
        };
    }
}
