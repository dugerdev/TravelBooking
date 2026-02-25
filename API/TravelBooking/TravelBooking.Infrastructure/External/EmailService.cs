using System.Net;
using System.Net.Mail;
using TravelBooking.Application.Abstractions.External;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace TravelBooking.Infrastructure.External;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    private readonly string? _smtpServer;
    private readonly int _smtpPort;
    private readonly string? _senderEmail;
    private readonly string _senderName;
    private readonly string? _smtpUsername;
    private readonly string? _smtpPassword;
    private readonly string _verificationUrl;
    private readonly string _resetPasswordUrl;
    private readonly bool _isEnabled;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        
        //---Email servisi opsiyonel - config bossa devre disi---//
        _smtpServer = configuration["Email:SmtpServer"];
        _smtpPort = configuration.GetValue<int>("Email:SmtpPort", 587);
        _senderEmail = configuration["Email:SenderEmail"];
        _senderName = configuration["Email:SenderName"] ?? "Gocebe";
        _smtpUsername = configuration["Email:Username"];
        _smtpPassword = configuration["Email:Password"];
        _verificationUrl = configuration["EmailVerification:VerificationUrl"] ?? "https://app.gocebe.com/verify-email";
        _resetPasswordUrl = configuration["PasswordReset:ResetUrl"] ?? "https://app.gocebe.com/reset-password";
        
        //---Email servisi aktif mi kontrol et (placeholder degerlerle devre disi)---//
        var hasValidCredentials = !string.IsNullOrWhiteSpace(_smtpUsername) && !string.IsNullOrWhiteSpace(_smtpPassword) &&
            !_smtpUsername.Contains("YOUR_", StringComparison.OrdinalIgnoreCase) &&
            !_smtpPassword.Contains("HERE", StringComparison.OrdinalIgnoreCase);
        _isEnabled = !string.IsNullOrWhiteSpace(_smtpServer) &&
                     !string.IsNullOrWhiteSpace(_senderEmail) &&
                     hasValidCredentials;
        
        if (!_isEnabled)
        {
            _logger.LogWarning("Email servisi devre disi. Email:SmtpServer, Email:SenderEmail, Email:Username veya Email:Password yapilandirilmamis.");
        }
        else
        {
            _logger.LogInformation("Email servisi aktif. SMTP Server: {SmtpServer}, Port: {Port}", _smtpServer, _smtpPort);
        }
    }

    public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true, CancellationToken cancellationToken = default)
    {
        if (!_isEnabled)
        {
            _logger.LogWarning("Email servisi devre disi. Email gonderilemedi: {To}, Subject: {Subject}", to, subject);
            return; //---Email gondermeyi sessizce atla---//
        }

        try
        {
            using var client = new SmtpClient(_smtpServer, _smtpPort)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_smtpUsername, _smtpPassword),
                DeliveryMethod = SmtpDeliveryMethod.Network
            };

            using var message = new MailMessage
            {
                From = new MailAddress(_senderEmail!, _senderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml
            };

            message.To.Add(to);

            await client.SendMailAsync(message, cancellationToken);
            _logger.LogInformation("Email sent to {Email}", to);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", to);
            //---Email gonderme hatasi uygulamayi durdurmamali---//
            //---Exception firlatma, sadece logla---//
        }
    }

    public async Task SendEmailVerificationAsync(string to, string userName, string verificationToken, CancellationToken cancellationToken = default)
    {
        var verificationLink = $"{_verificationUrl}?token={verificationToken}";
        var subject = "Email Verification - Gocebe";
        var body = EmailTemplates.GetEmailVerificationTemplate(userName, verificationLink);

        await SendEmailAsync(to, subject, body, true, cancellationToken);
    }

    public async Task SendPasswordResetAsync(string to, string userName, string resetToken, CancellationToken cancellationToken = default)
    {
        var resetLink = $"{_resetPasswordUrl}?token={resetToken}";
        var subject = "Password Reset - Gocebe";
        var body = EmailTemplates.GetPasswordResetTemplate(userName, resetLink);

        await SendEmailAsync(to, subject, body, true, cancellationToken);
    }

    public async Task SendReservationConfirmationAsync(string to, string userName, string pnr, decimal totalPrice, CancellationToken cancellationToken = default)
    {
        var subject = $"Rezervasyon Onayi - PNR: {pnr}";
        var body = EmailTemplates.GetReservationConfirmationTemplate(userName, pnr, totalPrice);

        await SendEmailAsync(to, subject, body, true, cancellationToken);
    }

    public async Task SendReservationCancellationAsync(string to, string userName, string pnr, CancellationToken cancellationToken = default)
    {
        var subject = $"Reservation Cancellation - PNR: {pnr}";
        var body = EmailTemplates.GetReservationCancellationTemplate(userName, pnr);

        await SendEmailAsync(to, subject, body, true, cancellationToken);
    }
}
