using TravelBooking.Domain.Common;

namespace TravelBooking.Domain.Entities;

/// <summary>
/// Represents a customer testimonial/review submitted through the website.
/// </summary>
public class Testimonial : BaseEntity, IAggregateRoot
{
    public string CustomerName { get; private set; } = string.Empty;
    public string Location { get; private set; } = string.Empty;
    public string Comment { get; private set; } = string.Empty;
    public int Rating { get; private set; }
    public string? AvatarUrl { get; private set; }
    public bool IsApproved { get; private set; } = false;
    public DateTime? ApprovedDate { get; private set; }
    public string? ApprovedBy { get; private set; }
    public string? RejectionReason { get; private set; }
    
    protected Testimonial() { }

    public Testimonial(
        string customerName,
        string location,
        string comment,
        int rating,
        string? avatarUrl = null)
    {
        if (string.IsNullOrWhiteSpace(customerName))
            throw new ArgumentException("Customer name cannot be empty.", nameof(customerName));
        if (string.IsNullOrWhiteSpace(comment))
            throw new ArgumentException("Comment cannot be empty.", nameof(comment));
        if (rating < 1 || rating > 5)
            throw new ArgumentException("Rating must be between 1 and 5.", nameof(rating));

        CustomerName = customerName.Trim();
        Location = location?.Trim() ?? string.Empty;
        Comment = comment.Trim();
        Rating = rating;
        AvatarUrl = avatarUrl?.Trim();
        IsApproved = false;
    }

    /// <summary>
    /// Approves the testimonial for display on the website.
    /// </summary>
    /// <param name="approvedBy">The ID of the administrator who approved.</param>
    public void Approve(string approvedBy)
    {
        if (IsApproved)
            return;

        IsApproved = true;
        ApprovedDate = DateTime.UtcNow;
        ApprovedBy = approvedBy;
        RejectionReason = null;
    }

    /// <summary>
    /// Rejects the testimonial with an optional reason.
    /// </summary>
    /// <param name="reason">The reason for rejection.</param>
    public void Reject(string? reason = null)
    {
        IsApproved = false;
        ApprovedDate = null;
        ApprovedBy = null;
        RejectionReason = reason;
    }

    /// <summary>
    /// Updates the testimonial information.
    /// </summary>
    public void Update(string customerName, string location, string comment, int rating, string? avatarUrl = null)
    {
        if (string.IsNullOrWhiteSpace(customerName))
            throw new ArgumentException("Customer name cannot be empty.", nameof(customerName));
        if (string.IsNullOrWhiteSpace(comment))
            throw new ArgumentException("Comment cannot be empty.", nameof(comment));
        if (rating < 1 || rating > 5)
            throw new ArgumentException("Rating must be between 1 and 5.", nameof(rating));

        CustomerName = customerName.Trim();
        Location = location?.Trim() ?? string.Empty;
        Comment = comment.Trim();
        Rating = rating;
        AvatarUrl = avatarUrl?.Trim();
    }
}
