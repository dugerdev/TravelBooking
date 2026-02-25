namespace TravelBooking.Application.Abstractions.External;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body, bool isHtml = true, CancellationToken cancellationToken = default);
    Task SendEmailVerificationAsync(string to, string userName, string verificationToken, CancellationToken cancellationToken = default);
    Task SendPasswordResetAsync(string to, string userName, string resetToken, CancellationToken cancellationToken = default);
    Task SendReservationConfirmationAsync(string to, string userName, string pnr, decimal totalPrice, CancellationToken cancellationToken = default);
    Task SendReservationCancellationAsync(string to, string userName, string pnr, CancellationToken cancellationToken = default);
}

