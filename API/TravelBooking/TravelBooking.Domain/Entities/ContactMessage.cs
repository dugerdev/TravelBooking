using TravelBooking.Domain.Common;

namespace TravelBooking.Domain.Entities;

/// <summary>
/// Represents a contact message submitted through the website contact form.
/// </summary>
public class ContactMessage : BaseEntity, IAggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public string Subject { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;
    public bool IsRead { get; private set; } = false;
    public DateTime? ReadDate { get; private set; }
    public string? ReadBy { get; private set; }
    public string? Response { get; private set; }
    public DateTime? ResponseDate { get; private set; }
    public string? ResponseBy { get; private set; }

    protected ContactMessage() { }

    public ContactMessage(
        string name,
        string email,
        string phone,
        string subject,
        string message)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty.", nameof(email));
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Message cannot be empty.", nameof(message));

        Name = name.Trim();
        Email = email.Trim();
        Phone = phone?.Trim() ?? string.Empty;
        Subject = subject?.Trim() ?? "General Inquiry";
        Message = message.Trim();
        IsRead = false;
    }

    /// <summary>
    /// Marks the message as read by an administrator.
    /// </summary>
    /// <param name="readBy">The ID of the administrator who read the message.</param>
    public void MarkAsRead(string readBy)
    {
        if (IsRead)
            return;

        IsRead = true;
        ReadDate = DateTime.UtcNow;
        ReadBy = readBy;
    }

    /// <summary>
    /// Marks the message as unread.
    /// </summary>
    public void MarkAsUnread()
    {
        IsRead = false;
        ReadDate = null;
        ReadBy = null;
    }

    /// <summary>
    /// Adds a response to the contact message.
    /// </summary>
    /// <param name="response">The response text.</param>
    /// <param name="responseBy">The ID of the administrator who responded.</param>
    public void AddResponse(string response, string responseBy)
    {
        if (string.IsNullOrWhiteSpace(response))
            throw new ArgumentException("Response cannot be empty.", nameof(response));

        Response = response.Trim();
        ResponseDate = DateTime.UtcNow;
        ResponseBy = responseBy;

        // Automatically mark as read when responding
        if (!IsRead)
            MarkAsRead(responseBy);
    }

    /// <summary>
    /// Updates the contact information.
    /// </summary>
    public void UpdateInfo(string name, string email, string phone, string subject, string message)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty.", nameof(email));
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Message cannot be empty.", nameof(message));

        Name = name.Trim();
        Email = email.Trim();
        Phone = phone?.Trim() ?? string.Empty;
        Subject = subject?.Trim() ?? "General Inquiry";
        Message = message.Trim();
    }
}
