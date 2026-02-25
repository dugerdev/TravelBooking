using System.ComponentModel.DataAnnotations;

namespace TravelBooking.Web.Models;

public class ContactMessage
{
    public Guid Id { get; set; }
    
    [Required(ErrorMessage = "Name is required")]
    public string Name { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string Email { get; set; } = string.Empty;
    
    [Phone(ErrorMessage = "Invalid phone number")]
    public string Phone { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Subject is required")]
    public string Subject { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Message is required")]
    public string Message { get; set; } = string.Empty;
    
    public bool IsRead { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ReadDate { get; set; }
    public string? ReadBy { get; set; }
    public string? Response { get; set; }
    public DateTime? ResponseDate { get; set; }
}

public class ContactMessageListViewModel
{
    public List<ContactMessage> Messages { get; set; } = new();
    public int TotalCount { get; set; }
    public int UnreadCount { get; set; }
    public string? StatusFilter { get; set; }
    public string? SearchQuery { get; set; }
}

public class ContactMessageDetailViewModel
{
    public ContactMessage Message { get; set; } = new();
    
    [Required(ErrorMessage = "Response is required")]
    public string? ResponseText { get; set; }
}
