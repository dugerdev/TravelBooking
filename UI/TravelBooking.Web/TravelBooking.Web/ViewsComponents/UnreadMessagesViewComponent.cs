using Microsoft.AspNetCore.Mvc;
using TravelBooking.Web.Services.ContactMessages;
using TravelBooking.Web.Models;

namespace TravelBooking.Web.ViewComponents;

public class UnreadMessagesViewComponent : ViewComponent
{
    private readonly IContactMessageService _messageService;

    public UnreadMessagesViewComponent(IContactMessageService messageService)
    {
        _messageService = messageService;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var unreadCount = await _messageService.GetUnreadCountAsync(HttpContext.RequestAborted);
        var (_, _, messages) = await _messageService.GetAllAsync("unread", null, HttpContext.RequestAborted);
        var recent = (messages ?? new List<ContactMessage>())
            .OrderByDescending(m => m.CreatedDate)
            .Take(5)
            .ToList();
        var model = new UnreadMessagesViewModel
        {
            UnreadCount = unreadCount,
            RecentMessages = recent
        };
        return View(model);
    }
}

public class UnreadMessagesViewModel
{
    public int UnreadCount { get; set; }
    public List<ContactMessage> RecentMessages { get; set; } = new();
}
