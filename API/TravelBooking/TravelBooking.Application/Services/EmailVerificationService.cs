using TravelBooking.Application.Abstractions.External;
using TravelBooking.Application.Common;
using TravelBooking.Application.Contracts;
using TravelBooking.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace TravelBooking.Application.Services;

public class EmailVerificationService : IEmailVerificationService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IEmailService _emailService;
    private readonly ILogger<EmailVerificationService> _logger;

    public EmailVerificationService(
        UserManager<AppUser> userManager,
        IEmailService emailService,
        ILogger<EmailVerificationService> logger)
    {
        _userManager = userManager;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<Result> SendVerificationEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            // Guvenlik: Email bulunamadiginda da basari don
            _logger.LogWarning("Verification email requested for non-existent email: {Email}", email);
            return new SuccessResult("Eger bu email adresi sistemde kayitliysa, dogrulama linki gonderildi.");
        }

        if (user.EmailConfirmed)
            return new ErrorResult("Email adresi zaten dogrulanmis.");

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        
        try
        {
            await _emailService.SendEmailVerificationAsync(user.Email!, user.UserName ?? "Kullanici", token, cancellationToken);
            _logger.LogInformation("Verification email sent to {Email}", email);
            return new SuccessResult("Email dogrulama linki email adresinize gonderildi.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send verification email to {Email}", email);
            //---Email servisi devre disiysa veya hata varsa, kullaniciya bilgi ver---//
            return new ErrorResult("Email gonderilirken bir hata olustu. Lutfen daha sonra tekrar deneyin veya yonetici ile iletisime gecin.");
        }
    }

    public async Task<Result> VerifyEmailAsync(string email, string token, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
            return new ErrorResult("Kullanici bulunamadi.");

        if (user.EmailConfirmed)
            return new ErrorResult("Email adresi zaten dogrulanmis.");

        var result = await _userManager.ConfirmEmailAsync(user, token);
        if (!result.Succeeded)
            return new ErrorResult(string.Join(", ", result.Errors.Select(e => e.Description)));

        _logger.LogInformation("Email verified for user {UserId}", user.Id);
        return new SuccessResult("Email adresi basariyla dogrulandi.");
    }
}
