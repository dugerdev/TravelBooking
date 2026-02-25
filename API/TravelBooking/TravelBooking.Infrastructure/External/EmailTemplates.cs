namespace TravelBooking.Infrastructure.External;

public static class EmailTemplates
{
    public static string GetEmailVerificationTemplate(string userName, string verificationLink)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .button {{ display: inline-block; padding: 12px 24px; background-color: #4CAF50; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Gocebe</h1>
        </div>
        <div class='content'>
            <h2>Hello {userName},</h2>
            <p>Click the button below to verify your email address:</p>
            <p style='text-align: center;'>
                <a href='{verificationLink}' class='button'>Verify Email</a>
            </p>
            <p>Or copy the link below into your browser:</p>
            <p style='word-break: break-all;'>{verificationLink}</p>
            <p>This link is valid for 24 hours.</p>
        </div>
        <div class='footer'>
            <p>This email was sent automatically. Please do not reply.</p>
        </div>
    </div>
</body>
</html>";
    }

    public static string GetPasswordResetTemplate(string userName, string resetLink)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #2196F3; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .button {{ display: inline-block; padding: 12px 24px; background-color: #2196F3; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
        .warning {{ color: #d32f2f; font-weight: bold; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Gocebe</h1>
        </div>
        <div class='content'>
            <h2>Hello {userName},</h2>
            <p>Click the button below to reset your password:</p>
            <p style='text-align: center;'>
                <a href='{resetLink}' class='button'>Reset Password</a>
            </p>
            <p>Or copy the link below into your browser:</p>
            <p style='word-break: break-all;'>{resetLink}</p>
            <p class='warning'>This link is valid for 1 hour.</p>
            <p>If you did not perform this action, you can ignore this email.</p>
        </div>
        <div class='footer'>
            <p>This email was sent automatically. Please do not reply.</p>
        </div>
    </div>
</body>
</html>";
    }

    public static string GetReservationConfirmationTemplate(string userName, string pnr, decimal totalPrice)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .info-box {{ background-color: white; padding: 15px; border-left: 4px solid #4CAF50; margin: 15px 0; }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Gocebe - Reservation Confirmation</h1>
        </div>
        <div class='content'>
            <h2>Hello {userName},</h2>
            <p>Your reservation has been created successfully.</p>
            <div class='info-box'>
                <p><strong>PNR:</strong> {pnr}</p>
                <p><strong>Total Amount:</strong> {totalPrice:C}</p>
            </div>
            <p>You can view your reservation details on our website or mobile app.</p>
        </div>
        <div class='footer'>
            <p>This email was sent automatically. Please do not reply.</p>
        </div>
    </div>
</body>
</html>";
    }

    public static string GetReservationCancellationTemplate(string userName, string pnr)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #f44336; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .info-box {{ background-color: white; padding: 15px; border-left: 4px solid #f44336; margin: 15px 0; }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Gocebe - Reservation Cancellation</h1>
        </div>
        <div class='content'>
            <h2>Hello {userName},</h2>
            <p>Your reservation has been cancelled.</p>
            <div class='info-box'>
                <p><strong>PNR:</strong> {pnr}</p>
            </div>
            <p>Please contact our customer service for refund information.</p>
        </div>
        <div class='footer'>
            <p>This email was sent automatically. Please do not reply.</p>
        </div>
    </div>
</body>
</html>";
    }
}
