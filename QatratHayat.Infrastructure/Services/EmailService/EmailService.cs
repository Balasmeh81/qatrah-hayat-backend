using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using QatratHayat.Application.Common.Exceptions;
using QatratHayat.Application.Common.Interfaces;
using QatratHayat.Application.Common.Settings;

namespace QatratHayat.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings emailSettings;
        private readonly ILogger<EmailService> logger;

        public EmailService(
            IOptions<EmailSettings> emailOptions,
            ILogger<EmailService> logger)
        {
            emailSettings = emailOptions.Value;
            this.logger = logger;

            ValidateSettings();
        }

        public async Task SendPasswordResetOtpAsync(
            string toEmail,
            string fullName,
            string otp,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var message = new MimeMessage();

                message.From.Add(new MailboxAddress(
                    emailSettings.SenderName,
                    emailSettings.SenderEmail
                ));

                message.To.Add(new MailboxAddress(fullName, toEmail));

                message.Subject = "Qatrat Hayat Password Reset Code";

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = BuildPasswordResetOtpHtml(fullName, otp),
                    TextBody = BuildPasswordResetOtpText(fullName, otp)
                };

                message.Body = bodyBuilder.ToMessageBody();

                using var smtpClient = new SmtpClient();

                await smtpClient.ConnectAsync(
                    emailSettings.SmtpHost,
                    emailSettings.SmtpPort,
                    SecureSocketOptions.StartTls,
                    cancellationToken
                );

                await smtpClient.AuthenticateAsync(
                    emailSettings.Username,
                    emailSettings.Password,
                    cancellationToken
                );

                await smtpClient.SendAsync(message, cancellationToken);

                await smtpClient.DisconnectAsync(true, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Failed to send password reset OTP email to {Email}",
                    toEmail
                );

                throw new BadRequestException(
                    "Failed to send password reset email.",
                    ErrorCodes.EmailSendingFailed
                );
            }
        }

        private void ValidateSettings()
        {
            if (string.IsNullOrWhiteSpace(emailSettings.SmtpHost))
                throw new InvalidOperationException("Email SMTP host is missing.");

            if (emailSettings.SmtpPort <= 0)
                throw new InvalidOperationException("Email SMTP port is invalid.");

            if (string.IsNullOrWhiteSpace(emailSettings.SenderName))
                throw new InvalidOperationException("Email sender name is missing.");

            if (string.IsNullOrWhiteSpace(emailSettings.SenderEmail))
                throw new InvalidOperationException("Email sender address is missing.");

            if (string.IsNullOrWhiteSpace(emailSettings.Username))
                throw new InvalidOperationException("Email SMTP username is missing.");

            if (string.IsNullOrWhiteSpace(emailSettings.Password))
                throw new InvalidOperationException("Email SMTP password is missing.");
        }

        private static string BuildPasswordResetOtpText(string fullName, string otp)
        {
            return $@"
Hello {fullName},

Your Qatrat Hayat password reset verification code is: {otp}

This code will expire in 10 minutes.

If you did not request a password reset, please ignore this email.

Qatrat Hayat Team
";
        }

        private static string BuildPasswordResetOtpHtml(string fullName, string otp)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <title>Password Reset Code</title>
</head>
<body style='margin:0; padding:0; background-color:#f7f7f7; font-family:Arial, sans-serif;'>
    <table width='100%' cellpadding='0' cellspacing='0' style='background-color:#f7f7f7; padding:30px 0;'>
        <tr>
            <td align='center'>
                <table width='600' cellpadding='0' cellspacing='0' style='background:#ffffff; border-radius:12px; overflow:hidden;'>
                    <tr>
                        <td style='background:#BF0007; color:#ffffff; padding:24px; text-align:center;'>
                            <h1 style='margin:0; font-size:24px;'>Qatrat Hayat</h1>
                        </td>
                    </tr>

                    <tr>
                        <td style='padding:32px; color:#101727;'>
                            <h2 style='margin-top:0;'>Password Reset Code</h2>

                            <p>Hello {fullName},</p>

                            <p>
                                We received a request to reset your password.
                                Use the verification code below to continue:
                            </p>

                            <div style='text-align:center; margin:32px 0;'>
                                <span style='
                                    display:inline-block;
                                    font-size:32px;
                                    letter-spacing:8px;
                                    font-weight:bold;
                                    color:#BF0007;
                                    background:#F7EBEB;
                                    padding:16px 24px;
                                    border-radius:10px;
                                '>
                                    {otp}
                                </span>
                            </div>

                            <p>This code will expire in <strong>10 minutes</strong>.</p>

                            <p style='color:#666666; font-size:14px;'>
                                If you did not request a password reset, you can safely ignore this email.
                            </p>

                            <p style='margin-bottom:0;'>Qatrat Hayat Team</p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
        }
    }
}