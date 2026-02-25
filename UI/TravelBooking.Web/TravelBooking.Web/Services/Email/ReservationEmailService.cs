using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace TravelBooking.Web.Services.Email;

public class ReservationEmailService : IReservationEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ReservationEmailService> _logger;
    private readonly string? _smtpServer;
    private readonly int _smtpPort;
    private readonly string? _senderEmail;
    private readonly string _senderName;
    private readonly string? _smtpUsername;
    private readonly string? _smtpPassword;
    private readonly bool _isEnabled;

    public ReservationEmailService(IConfiguration configuration, ILogger<ReservationEmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _smtpServer = configuration["Email:SmtpServer"];
        _smtpPort = configuration.GetValue<int>("Email:SmtpPort", 587);
        _senderEmail = configuration["Email:SenderEmail"];
        _senderName = configuration["Email:SenderName"] ?? "TravelBooking";
        _smtpUsername = configuration["Email:Username"];
        _smtpPassword = configuration["Email:Password"];

        _isEnabled = !string.IsNullOrWhiteSpace(_smtpServer) &&
                     !string.IsNullOrWhiteSpace(_senderEmail) &&
                     !string.IsNullOrWhiteSpace(_smtpUsername) &&
                     !string.IsNullOrWhiteSpace(_smtpPassword);

        if (!_isEnabled)
            _logger.LogWarning("E-posta servisi devre disi. Email:SmtpServer, SenderEmail, Username veya Password yapilandirilmamis.");
    }

    public async Task SendFlightBookingConfirmationAsync(string toEmail, string passengerName, string pnr, decimal totalPrice, string currency, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(toEmail))
        {
            _logger.LogWarning("Rezervasyon onay e-postasi atlaniyor: alici e-posta bos.");
            return;
        }

        if (!_isEnabled)
        {
            _logger.LogWarning("E-posta servisi devre disi. Rezervasyon onayi gonderilemedi: {To}", toEmail);
            return;
        }

        var subject = $"Your ticket has been purchased successfully - PNR: {pnr}";
        var body = GetBookingConfirmationHtml(passengerName, pnr, totalPrice, currency);

        try
        {
            using var client = new SmtpClient(_smtpServer, _smtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(_smtpUsername, _smtpPassword)
            };

            using var message = new MailMessage
            {
                From = new MailAddress(_senderEmail!, _senderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            message.To.Add(toEmail.Trim());

            await client.SendMailAsync(message, cancellationToken);
            _logger.LogInformation("Rezervasyon onay e-postasi gonderildi: {Email}, PNR: {Pnr}", toEmail, pnr);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Rezervasyon onay e-postasi gonderilemedi: {Email}", toEmail);
            // Uygulamayi durdurmuyoruz, sadece logluyoruz
        }
    }

    public async Task SendDetailedFlightBookingConfirmationAsync(string toEmail, string passengerName, string pnr,
        string flightNumber, string from, string to, DateTime departureDate, DateTime arrivalDate,
        decimal totalPrice, string currency, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(toEmail))
        {
            _logger.LogWarning("Rezervasyon onay e-postasi atlaniyor: alici e-posta bos.");
            return;
        }

        if (!_isEnabled)
        {
            _logger.LogWarning("E-posta servisi devre disi. Rezervasyon onayi gonderilemedi: {To}", toEmail);
            return;
        }

        var subject = $"✈️ Ucus Rezervasyonunuz Onaylandi - PNR: {pnr}";
        var body = GetDetailedBookingConfirmationHtml(passengerName, pnr, flightNumber, from, to, 
            departureDate, arrivalDate, totalPrice, currency);

        try
        {
            using var client = new SmtpClient(_smtpServer, _smtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(_smtpUsername, _smtpPassword)
            };

            using var message = new MailMessage
            {
                From = new MailAddress(_senderEmail!, _senderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            message.To.Add(toEmail.Trim());

            await client.SendMailAsync(message, cancellationToken);
            _logger.LogInformation("Detayli rezervasyon onay e-postasi gonderildi: {Email}, PNR: {Pnr}", toEmail, pnr);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Rezervasyon onay e-postasi gonderilemedi: {Email}", toEmail);
        }
    }

    private static string GetBookingConfirmationHtml(string userName, string pnr, decimal totalPrice, string currency)
    {
        var priceStr = $"{totalPrice:N2} {currency}";
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #4D73FC 0%, #3a5dd9 100%); color: white; padding: 30px; text-align: center; }}
        .content {{ padding: 30px; background-color: #f9f9f9; }}
        .info-box {{ background-color: white; padding: 20px; border-left: 4px solid #4D73FC; margin: 20px 0; border-radius: 5px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }}
        .pnr-code {{ background-color: #4D73FC; color: white; padding: 15px; text-align: center; font-size: 24px; font-weight: bold; letter-spacing: 3px; border-radius: 5px; margin: 20px 0; }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
        .button {{ display: inline-block; padding: 12px 30px; background-color: #4D73FC; color: white; text-decoration: none; border-radius: 5px; margin: 10px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>✈️ TravelBooking - Flight Confirmation</h1>
        </div>
        <div class='content'>
            <h2>Hello {userName},</h2>
            <p><strong>Your flight reservation has been completed successfully!</strong></p>
            <div class='pnr-code'>
                {pnr}
            </div>
            <div class='info-box'>
                <p style='margin: 10px 0;'><strong>📋 Your PNR Code:</strong> {pnr}</p>
                <p style='margin: 10px 0;'><strong>💰 Total Amount:</strong> {priceStr}</p>
                <p style='margin: 10px 0; color: #666;'>Please keep your PNR code for check-in.</p>
            </div>
            <p><strong>Important Information:</strong></p>
            <ul>
                <li>Arrive at the airport at least 2 hours before your flight</li>
                <li>Keep your ID documents with you</li>
                <li>You can use your PNR code for online check-in</li>
            </ul>
            <p>You can view your reservation details from the <strong>""My Reservations""</strong> section in your account.</p>
        </div>
        <div class='footer'>
            <p>This email was sent automatically. Please do not reply.</p>
            <p>© 2024 TravelBooking. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
    }

    private static string GetDetailedBookingConfirmationHtml(string userName, string pnr, string flightNumber,
        string from, string to, DateTime departureDate, DateTime arrivalDate, decimal totalPrice, string currency)
    {
        var priceStr = $"{totalPrice:N2} {currency}";
        var departureStr = departureDate.ToString("dd MMMM yyyy, HH:mm");
        var arrivalStr = arrivalDate.ToString("dd MMMM yyyy, HH:mm");
        var duration = arrivalDate - departureDate;
        var durationStr = $"{(int)duration.TotalHours}s {duration.Minutes}dk";

        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; line-height: 1.6; color: #333; margin: 0; padding: 0; }}
        .container {{ max-width: 600px; margin: 0 auto; }}
        .header {{ background: linear-gradient(135deg, #4D73FC 0%, #3a5dd9 100%); color: white; padding: 40px 20px; text-align: center; }}
        .header h1 {{ margin: 0; font-size: 28px; }}
        .success-icon {{ font-size: 48px; margin-bottom: 10px; }}
        .content {{ padding: 30px 20px; background-color: #ffffff; }}
        .pnr-section {{ background: linear-gradient(135deg, #4D73FC 0%, #3a5dd9 100%); color: white; padding: 25px; text-align: center; border-radius: 10px; margin: 25px 0; }}
        .pnr-label {{ font-size: 14px; opacity: 0.9; margin-bottom: 5px; }}
        .pnr-code {{ font-size: 32px; font-weight: bold; letter-spacing: 8px; }}
        .flight-card {{ background-color: #f8f9fa; border: 2px solid #e9ecef; border-radius: 10px; padding: 25px; margin: 20px 0; }}
        .flight-route {{ display: table; width: 100%; margin: 20px 0; }}
        .flight-point {{ display: table-cell; width: 40%; vertical-align: top; }}
        .flight-arrow {{ display: table-cell; width: 20%; text-align: center; vertical-align: middle; font-size: 24px; color: #4D73FC; }}
        .city-code {{ font-size: 28px; font-weight: bold; color: #212529; margin-bottom: 5px; }}
        .datetime {{ color: #6c757d; font-size: 14px; }}
        .info-row {{ display: flex; justify-content: space-between; padding: 12px 0; border-bottom: 1px solid #e9ecef; }}
        .info-row:last-child {{ border-bottom: none; }}
        .info-label {{ color: #6c757d; }}
        .info-value {{ font-weight: 600; color: #212529; }}
        .price-section {{ background-color: #e7f1ff; border-left: 4px solid #4D73FC; padding: 20px; border-radius: 5px; margin: 20px 0; }}
        .price-total {{ font-size: 24px; font-weight: bold; color: #4D73FC; }}
        .important-info {{ background-color: #fff3cd; border-left: 4px solid #ffc107; padding: 15px; border-radius: 5px; margin: 20px 0; }}
        .checklist {{ list-style: none; padding: 0; }}
        .checklist li {{ padding: 8px 0; padding-left: 25px; position: relative; }}
        .checklist li:before {{ content: '✓'; position: absolute; left: 0; color: #28a745; font-weight: bold; }}
        .footer {{ background-color: #f8f9fa; padding: 20px; text-align: center; font-size: 12px; color: #6c757d; }}
        .button {{ display: inline-block; padding: 14px 32px; background-color: #4D73FC; color: white; text-decoration: none; border-radius: 5px; margin: 15px 0; font-weight: 600; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <div class='success-icon'>✈️</div>
            <h1>Ucus Rezervasyonunuz Onaylandi!</h1>
            <p style='margin: 10px 0 0 0; opacity: 0.9;'>TravelBooking ile guvenli yolculuklar</p>
        </div>
        
        <div class='content'>
            <h2 style='color: #212529; margin-top: 0;'>Hello {userName},</h2>
            <p style='font-size: 16px;'>Your flight reservation has been completed successfully. Your ticket has been sent to your email address.</p>
            
            <div class='pnr-section'>
                <div class='pnr-label'>REZERVASYON KODU (PNR)</div>
                <div class='pnr-code'>{pnr}</div>
            </div>

            <div class='flight-card'>
                <h3 style='margin-top: 0; color: #212529;'>Flight Details</h3>
                
                <div class='flight-route'>
                    <div class='flight-point'>
                        <div class='city-code'>{from}</div>
                        <div class='datetime'>{departureStr}</div>
                    </div>
                    <div class='flight-arrow'>✈️</div>
                    <div class='flight-point' style='text-align: right;'>
                        <div class='city-code'>{to}</div>
                        <div class='datetime'>{arrivalStr}</div>
                    </div>
                </div>

                <div style='margin-top: 20px;'>
                    <div class='info-row'>
                        <span class='info-label'>Ucus Numarasi</span>
                        <span class='info-value'>{flightNumber}</span>
                    </div>
                    <div class='info-row'>
                        <span class='info-label'>Flight Duration</span>
                        <span class='info-value'>{durationStr}</span>
                    </div>
                </div>
            </div>

            <div class='price-section'>
                <div style='display: flex; justify-content: space-between; align-items: center;'>
                    <span style='font-size: 16px; color: #495057;'>Toplam Tutar</span>
                    <span class='price-total'>{priceStr}</span>
                </div>
            </div>

            <div class='important-info'>
                <h4 style='margin-top: 0; color: #856404;'>⚠️ Important Information</h4>
                <ul class='checklist'>
                    <li>Arrive at the airport at least <strong>2 hours before</strong> your flight</li>
                    <li>Keep a valid <strong>ID document</strong> or passport with you</li>
                    <li>You can use your <strong>PNR code</strong> for online check-in</li>
                    <li>Check airline rules for baggage limits</li>
                </ul>
            </div>

            <div style='text-align: center; margin: 30px 0;'>
                <p style='color: #6c757d;'>Rezervasyon detaylarinizi hesabinizdan goruntuleyebilirsiniz</p>
            </div>
        </div>

        <div class='footer'>
            <p style='margin: 5px 0;'><strong>Contact:</strong> support@gocebe.com</p>
            <p style='margin: 5px 0;'>This email was sent automatically. Please do not reply.</p>
            <p style='margin: 15px 0 5px 0;'>© 2024 Gocebe. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
    }
}
