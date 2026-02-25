using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelBooking.Web.Services.Admin;
using TravelBooking.Web.ViewModels.Admin;

namespace TravelBooking.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class UsersAdminController : Controller
{
    private readonly IAdminService _adminService;

    public UsersAdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
    {
        var (success, message, data) = await _adminService.GetUsersPagedAsync(pageNumber, pageSize, ct);
        
        if (!success)
        {
            TempData["Error"] = message;
            return View(new DTOs.Common.PagedResultDto<UserDto>());
        }

        return View(data);
    }

    public async Task<IActionResult> Details(string id, CancellationToken ct = default)
    {
        var (success, message, user) = await _adminService.GetUserByIdAsync(id, ct);
        
        if (!success || user == null)
        {
            TempData["Error"] = message;
            return RedirectToAction(nameof(Index));
        }

        return View(user);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string id, CancellationToken ct = default)
    {
        var (success, message, user) = await _adminService.GetUserByIdAsync(id, ct);
        
        if (!success || user == null)
        {
            TempData["Error"] = message;
            return RedirectToAction(nameof(Index));
        }

        var model = new EditUserViewModel
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditUserViewModel model, CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
            return View(model);

        var dto = new UpdateUserDto
        {
            UserName = model.UserName,
            Email = model.Email,
            PhoneNumber = model.PhoneNumber
        };

        var (success, message) = await _adminService.UpdateUserAsync(model.Id, dto, ct);
        
        if (!success)
        {
            ModelState.AddModelError("", message);
            return View(model);
        }

        TempData["Success"] = "Kullanıcı başarıyla güncellendi.";
        return RedirectToAction(nameof(Details), new { id = model.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Delete(string id, CancellationToken ct = default)
    {
        var (success, message, user) = await _adminService.GetUserByIdAsync(id, ct);
        
        if (!success || user == null)
        {
            TempData["Error"] = message;
            return RedirectToAction(nameof(Index));
        }

        return View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string id, CancellationToken ct = default)
    {
        var (success, message) = await _adminService.DeleteUserAsync(id, ct);
        
        if (!success)
        {
            TempData["Error"] = message;
            return RedirectToAction(nameof(Delete), new { id });
        }

        TempData["Success"] = "Kullanıcı başarıyla silindi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Lock(string id, CancellationToken ct = default)
    {
        var lockoutEnd = DateTime.UtcNow.AddDays(30);
        var (success, message) = await _adminService.LockUserAsync(id, lockoutEnd, ct);
        
        if (!success)
        {
            TempData["Error"] = message;
        }
        else
        {
            TempData["Success"] = "Kullanıcı kilitlendi.";
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unlock(string id, CancellationToken ct = default)
    {
        var (success, message) = await _adminService.UnlockUserAsync(id, ct);
        
        if (!success)
        {
            TempData["Error"] = message;
        }
        else
        {
            TempData["Success"] = "Kullanıcı kilidi açıldı.";
        }

        return RedirectToAction(nameof(Details), new { id });
    }
}
