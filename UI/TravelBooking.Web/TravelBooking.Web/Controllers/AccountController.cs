using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelBooking.Web.Services.Account;
using TravelBooking.Web.Services.Auth;
using TravelBooking.Web.Services.Reservations;
using TravelBooking.Web.ViewModels.Auth;
using TravelBooking.Web.ViewModels.Account;
using TravelBooking.Web.DTOs.Account;

namespace TravelBooking.Web.Controllers;

public class AccountController : Controller
{
    private readonly IAuthService _authService;
    private readonly IAccountService _accountService;
    private readonly IReservationService _reservationService;

    public AccountController(IAuthService authService, IAccountService accountService, IReservationService reservationService)
    {
        _authService = authService;
        _accountService = accountService;
        _reservationService = reservationService;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (_authService.IsAuthenticated())
            return RedirectToAction("Index", "Home");
        ViewData["ReturnUrl"] = returnUrl ?? "/";
        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null, CancellationToken ct = default)
    {
        ViewData["ReturnUrl"] = returnUrl ?? "/";
        if (!ModelState.IsValid)
            return View(model);

        var (success, message, token) = await _authService.LoginAsync(model.UserNameOrEmail, model.Password, ct);
        if (!success)
        {
            ModelState.AddModelError("", message);
            return View(model);
        }

        // Admin kullanicisini dashboard'a yonlendir (case-insensitive kontrol)
        if (token?.Roles != null && token.Roles.Any(r => r.Equals("Admin", StringComparison.OrdinalIgnoreCase)))
        {
            if (string.IsNullOrEmpty(returnUrl))
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
        }

        return LocalRedirect(returnUrl ?? "/");
    }

    [HttpGet]
    public IActionResult SignUp(string? returnUrl = null)
    {
        if (_authService.IsAuthenticated())
            return RedirectToAction("Index", "Home");
        ViewData["ReturnUrl"] = returnUrl ?? "/";
        return View(new SignUpViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SignUp(SignUpViewModel model, string? returnUrl = null, CancellationToken ct = default)
    {
        ViewData["ReturnUrl"] = returnUrl ?? "/";
        if (!ModelState.IsValid)
            return View(model);

        var (success, message, _) = await _authService.SignUpAsync(model.Email, model.UserName, model.Password, ct);
        if (!success)
        {
            ModelState.AddModelError("", message);
            return View(model);
        }
        return LocalRedirect(returnUrl ?? "/");
    }

    [HttpGet]
    public IActionResult Register(string? returnUrl = null) => RedirectToAction(nameof(SignUp), new { returnUrl });

    [HttpGet]
    public async Task<IActionResult> Profile(CancellationToken ct)
    {
        if (!_authService.IsAuthenticated())
            return RedirectToAction(nameof(Login), new { returnUrl = "/Account/Profile" });
        var (success, message, profile) = await _accountService.GetProfileAsync(ct);
        if (!success || profile == null)
            return RedirectToAction(nameof(Login));
        return View(profile);
    }

    [HttpGet]
    public async Task<IActionResult> MyReservations(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
    {
        if (!_authService.IsAuthenticated())
            return RedirectToAction(nameof(Login), new { returnUrl = "/Account/MyReservations" });
        var (success, message, paged) = await _reservationService.GetMyReservationsPagedAsync(pageNumber, pageSize, ct);
        if (!success)
            TempData["ReservationError"] = message;
        return View(paged ?? new DTOs.Common.PagedResultDto<DTOs.Reservations.ReservationDto>());
    }

    [HttpGet]
    public async Task<IActionResult> ReservationDetail(Guid id, CancellationToken ct = default)
    {
        if (!_authService.IsAuthenticated())
            return RedirectToAction(nameof(Login), new { returnUrl = "/Account/MyReservations" });
        var (success, message, reservation) = await _reservationService.GetByIdAsync(id, ct);
        if (!success || reservation == null)
        {
            TempData["ReservationError"] = message ?? "Reservation not found.";
            return RedirectToAction(nameof(MyReservations));
        }
        return View(reservation);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelReservation(Guid id, CancellationToken ct = default)
    {
        if (!_authService.IsAuthenticated())
            return RedirectToAction(nameof(Login), new { returnUrl = "/Account/MyReservations" });
        var (success, message) = await _reservationService.CancelAsync(id, ct);
        if (success)
            TempData["ReservationSuccess"] = message ?? "Your reservation has been cancelled.";
        else
            TempData["ReservationError"] = message ?? "Cancellation failed.";
        return RedirectToAction(nameof(MyReservations));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout(CancellationToken ct = default)
    {
        await _authService.LogoutAsync(ct);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public async Task<IActionResult> EditProfile(CancellationToken ct = default)
    {
        if (!_authService.IsAuthenticated())
            return RedirectToAction(nameof(Login), new { returnUrl = "/Account/EditProfile" });

        var (success, message, profile) = await _accountService.GetProfileAsync(ct);
        if (!success || profile == null)
        {
            TempData["Error"] = "Could not retrieve profile information.";
            return RedirectToAction(nameof(Profile));
        }

        var model = new EditProfileViewModel
        {
            UserName = profile.UserName,
            Email = profile.Email,
            PhoneNumber = profile.PhoneNumber
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProfile(EditProfileViewModel model, CancellationToken ct = default)
    {
        if (!_authService.IsAuthenticated())
            return RedirectToAction(nameof(Login));

        if (!ModelState.IsValid)
            return View(model);

        var dto = new UpdateProfileDto
        {
            UserName = model.UserName,
            Email = model.Email,
            PhoneNumber = model.PhoneNumber
        };

        var (success, message) = await _accountService.UpdateProfileAsync(dto, ct);
        
        if (!success)
        {
            ModelState.AddModelError("", message);
            return View(model);
        }

        TempData["Success"] = "Your profile has been updated successfully.";
        return RedirectToAction(nameof(Profile));
    }

    [HttpGet]
    public IActionResult ChangePassword()
    {
        if (!_authService.IsAuthenticated())
            return RedirectToAction(nameof(Login), new { returnUrl = "/Account/ChangePassword" });

        return View(new ChangePasswordViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model, CancellationToken ct = default)
    {
        if (!_authService.IsAuthenticated())
            return RedirectToAction(nameof(Login));

        if (!ModelState.IsValid)
            return View(model);

        var dto = new ChangePasswordDto
        {
            CurrentPassword = model.CurrentPassword,
            NewPassword = model.NewPassword,
            ConfirmPassword = model.ConfirmPassword
        };

        var (success, message) = await _accountService.ChangePasswordAsync(dto, ct);
        
        if (!success)
        {
            ModelState.AddModelError("", message);
            return View(model);
        }

        TempData["Success"] = "Sifreniz basariyla degistirildi.";
        return RedirectToAction(nameof(Profile));
    }

    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(string email, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            ModelState.AddModelError("", "Please enter your email address.");
            return View();
        }

        var (success, message) = await _accountService.ForgotPasswordAsync(email, ct);
        
        if (success)
        {
            TempData["Success"] = "A password reset link has been sent to your email address.";
            return RedirectToAction(nameof(Login));
        }

        // Don't reveal if email exists or not for security
        TempData["Success"] = "If an account is registered with this email address, a password reset link will be sent.";
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult ResetPassword(string token, string email)
    {
        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(email))
        {
            TempData["ErrorMessage"] = "Invalid password reset link.";
            return RedirectToAction(nameof(Login));
        }

        var model = new ResetPasswordViewModel
        {
            Token = token,
            Email = email
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model, CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
            return View(model);

        var (success, message) = await _accountService.ResetPasswordAsync(model.Email, model.Token, model.NewPassword, ct);
        
        if (!success)
        {
            ModelState.AddModelError("", message);
            return View(model);
        }

        TempData["Success"] = "Your password has been reset successfully. You can now log in with your new password.";
        return RedirectToAction(nameof(Login));
    }
}
