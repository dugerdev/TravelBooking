using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelBooking.Web.Services.ContactMessages;
using TravelBooking.Web.Models;

namespace TravelBooking.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class MessagesAdminController : Controller
{
    private readonly IContactMessageService _contactMessageService;

    public MessagesAdminController(IContactMessageService contactMessageService)
    {
        _contactMessageService = contactMessageService;
    }

    public async Task<IActionResult> Index(string? status, string? search, CancellationToken ct = default)
    {
        if (User?.Identity?.IsAuthenticated != true)
            return RedirectToAction("Login", "Account", new { area = "" });
        if (!User.IsInRole("Admin"))
            return RedirectToAction("Index", "Home", new { area = "" });

        var (success, message, messages) = await _contactMessageService.GetAllAsync(status, search, ct);
        
        if (!success)
        {
            TempData["ErrorMessage"] = message;
            messages = new List<ContactMessage>();
        }

        var unreadCount = await _contactMessageService.GetUnreadCountAsync(ct);

        var viewModel = new ContactMessageListViewModel
        {
            Messages = messages ?? new List<ContactMessage>(),
            TotalCount = messages?.Count ?? 0,
            UnreadCount = unreadCount,
            StatusFilter = status,
            SearchQuery = search
        };

        return View(viewModel);
    }

    public async Task<IActionResult> Detail(Guid id, CancellationToken ct = default)
    {
        if (User?.Identity?.IsAuthenticated != true)
            return RedirectToAction("Login", "Account", new { area = "" });
        if (!User.IsInRole("Admin"))
            return RedirectToAction("Index", "Home", new { area = "" });

        var (success, message, contactMessage) = await _contactMessageService.GetByIdAsync(id, ct);
        
        if (!success || contactMessage == null)
        {
            TempData["ErrorMessage"] = message;
            return RedirectToAction(nameof(Index));
        }

        // Auto-mark as read when viewing
        if (!contactMessage.IsRead)
        {
            var userName = User.Identity?.Name ?? "Admin";
            await _contactMessageService.MarkAsReadAsync(id, userName, ct);
        }

        var viewModel = new ContactMessageDetailViewModel
        {
            Message = contactMessage
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken ct = default)
    {
        if (User?.Identity?.IsAuthenticated != true)
            return RedirectToAction("Login", "Account", new { area = "" });
        if (!User.IsInRole("Admin"))
            return RedirectToAction("Index", "Home", new { area = "" });

        var userName = User.Identity?.Name ?? "Admin";
        var (success, message) = await _contactMessageService.MarkAsReadAsync(id, userName, ct);
        
        if (success)
            TempData["SuccessMessage"] = message;
        else
            TempData["ErrorMessage"] = message;

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddResponse(Guid id, string responseText, CancellationToken ct = default)
    {
        if (User?.Identity?.IsAuthenticated != true)
            return RedirectToAction("Login", "Account", new { area = "" });
        if (!User.IsInRole("Admin"))
            return RedirectToAction("Index", "Home", new { area = "" });

        if (string.IsNullOrWhiteSpace(responseText))
        {
            TempData["ErrorMessage"] = "Response cannot be empty.";
            return RedirectToAction(nameof(Detail), new { id });
        }

        var userName = User.Identity?.Name ?? "Admin";
        var (success, message) = await _contactMessageService.AddResponseAsync(id, responseText, userName, ct);
        
        if (success)
            TempData["SuccessMessage"] = message;
        else
            TempData["ErrorMessage"] = message;

        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        if (User?.Identity?.IsAuthenticated != true)
            return RedirectToAction("Login", "Account", new { area = "" });
        if (!User.IsInRole("Admin"))
            return RedirectToAction("Index", "Home", new { area = "" });

        var (success, message) = await _contactMessageService.DeleteAsync(id, ct);
        
        if (success)
            TempData["SuccessMessage"] = message;
        else
            TempData["ErrorMessage"] = message;

        return RedirectToAction(nameof(Index));
    }
}
