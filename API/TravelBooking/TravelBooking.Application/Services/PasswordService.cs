using TravelBooking.Application.Abstractions.External;
using TravelBooking.Application.Common;
using TravelBooking.Application.Contracts;
using TravelBooking.Application.Dtos;
using TravelBooking.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace TravelBooking.Application.Services;

public class PasswordService : IPasswordService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IEmailService _emailService;
    private readonly ILogger<PasswordService> _logger;

    public PasswordService(
        UserManager<AppUser> userManager,
        IEmailService emailService,
        ILogger<PasswordService> logger)
    {
        _userManager = userManager;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<Result> ChangePasswordAsync(string userId, ChangePasswordDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.NewPassword != dto.ConfirmPassword)
            return new ErrorResult("Yeni sifre ve sifre onayi eslesmiyor.");

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return new ErrorResult("Kullanici bulunamadi.");

        var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
        if (!result.Succeeded)
            return new ErrorResult(string.Join(", ", result.Errors.Select(e => e.Description)));

        _logger.LogInformation("Password changed for user {UserId}", userId);
        return new SuccessResult("Sifre basariyla degistirildi.");
    }

    public async Task<Result> ResetPasswordRequestAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            // Guvenlik: Email bulunamadiginda da basari don (email enumeration onleme)
            _logger.LogWarning("Password reset requested for non-existent email: {Email}", email);
            return new SuccessResult("Eger bu email adresi sistemde kayitliysa, sifre sifirlama linki gonderildi.");
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        
        try
        {
            await _emailService.SendPasswordResetAsync(user.Email!, user.UserName ?? "Kullanici", token, cancellationToken);
            _logger.LogInformation("Password reset email sent to {Email}", email);
            return new SuccessResult("Sifre sifirlama linki email adresinize gonderildi.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send password reset email to {Email}", email);
            //---Email servisi devre disiysa veya hata varsa, kullaniciya bilgi ver---//
            return new ErrorResult("Email gonderilirken bir hata olustu. Lutfen daha sonra tekrar deneyin veya yonetici ile iletisime gecin.");
        }
    }

    public async Task<Result> ResetPasswordAsync(ResetPasswordDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.NewPassword != dto.ConfirmPassword)
            return new ErrorResult("Yeni sifre ve sifre onayi eslesmiyor.");

        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null)
            return new ErrorResult("Kullanici bulunamadi.");

        var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
        if (!result.Succeeded)
            return new ErrorResult(string.Join(", ", result.Errors.Select(e => e.Description)));

        _logger.LogInformation("Password reset completed for user {UserId}", user.Id);
        return new SuccessResult("Sifre basariyla sifirlandi.");
    }
}
