using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Utils;
using QatratHayat.Application.Common.Exceptions;
using QatratHayat.Application.Common.Interfaces;
using QatratHayat.Application.Common.Settings;

namespace QatratHayat.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings emailSettings;
        private readonly ILogger<EmailService> logger;

        public EmailService(IOptions<EmailSettings> emailOptions, ILogger<EmailService> logger)
        {
            emailSettings = emailOptions.Value;
            this.logger = logger;

            ValidateSettings();
        }

        public async Task SendPasswordResetOtpAsync(
            string toEmail,
            string fullName,
            string otp,
            bool isArabic,
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                var message = new MimeMessage();

                message.From.Add(
                    new MailboxAddress(emailSettings.SenderName, emailSettings.SenderEmail)
                );

                message.To.Add(new MailboxAddress(fullName, toEmail));

                message.Subject = isArabic
                    ? "رمز إعادة تعيين كلمة المرور - قطرة حياة"
                    : "Password Reset Code - Qatrat Hayat";

                var bodyBuilder = new BodyBuilder();

                var logoFileName = isArabic
                    ? "Qatrah_Hayat_logo_ar.svg"
                    : "Qatrah_Hayat_logo_en.svg";

                var logoPath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "assets",
                    "images",
                    logoFileName
                );

                string? logoContentId = null;

                if (File.Exists(logoPath))
                {
                    var logo = bodyBuilder.LinkedResources.Add(logoPath);
                    logo.ContentId = MimeUtils.GenerateMessageId();
                    logoContentId = logo.ContentId;
                }

                bodyBuilder.HtmlBody = BuildPasswordResetOtpHtml(
                    fullName,
                    otp,
                    isArabic,
                    logoContentId
                );

                bodyBuilder.TextBody = BuildPasswordResetOtpText(fullName, otp, isArabic);

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
                logger.LogError(ex, "Failed to send password reset OTP email to {Email}", toEmail);

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

        private static string BuildPasswordResetOtpText(string fullName, string otp, bool isArabic)
        {
            if (isArabic)
            {
                return $@"
مرحبًا {fullName}،

استلمنا طلبًا لإعادة تعيين كلمة المرور الخاصة بحسابك في نظام قطرة حياة.

رمز التحقق الخاص بك هو:
{otp}

تنتهي صلاحية هذا الرمز خلال 10 دقائق.

إذا لم تطلب إعادة تعيين كلمة المرور، يمكنك تجاهل هذه الرسالة بأمان.
لا تشارك هذا الرمز مع أي شخص.

فريق قطرة حياة
";
            }

            return $@"
Hello {fullName},

We received a request to reset the password for your Qatrat Hayat account.

Your verification code is:
{otp}

This code will expire in 10 minutes.

If you did not request a password reset, you can safely ignore this email.
Do not share this code with anyone.

Qatrat Hayat Team
";
        }

        private static string BuildPasswordResetOtpHtml(
            string fullName,
            string otp,
            bool isArabic,
            string? logoContentId
        )
        {
            var htmlLang = isArabic ? "ar" : "en";
            var direction = isArabic ? "rtl" : "ltr";
            var textAlign = isArabic ? "right" : "left";
            var borderSide = isArabic ? "right" : "left";

            var appName = isArabic ? "قطرة حياة" : "Qatrat Hayat";

            var pageTitle = isArabic
                ? "رمز إعادة تعيين كلمة المرور - قطرة حياة"
                : "Password Reset Code - Qatrat Hayat";

            var headerSubtitle = isArabic ? "رمز إعادة تعيين كلمة المرور" : "Password Reset Code";

            var mainTitle = isArabic ? "إعادة تعيين كلمة المرور" : "Reset Password";

            var greeting = isArabic ? $"مرحبًا {fullName}،" : $"Hello {fullName},";

            var introText = isArabic
                ? "استلمنا طلبًا لإعادة تعيين كلمة المرور الخاصة بحسابك في نظام قطرة حياة. استخدم رمز التحقق أدناه للمتابعة."
                : "We received a request to reset the password for your Qatrat Hayat account. Use the verification code below to continue.";

            var expiryText = isArabic ? "تنتهي صلاحية هذا الرمز خلال" : "This code will expire in";

            var expiryDuration = isArabic ? "10 دقائق" : "10 minutes";

            var securityNote = isArabic
                ? "إذا لم تطلب إعادة تعيين كلمة المرور، يمكنك تجاهل هذه الرسالة بأمان. لا تشارك هذا الرمز مع أي شخص."
                : "If you did not request a password reset, you can safely ignore this email. Do not share this code with anyone.";

            var teamName = isArabic ? "فريق قطرة حياة" : "Qatrat Hayat Team";

            var automaticMessage = isArabic
                ? "هذه رسالة تلقائية، يرجى عدم الرد عليها."
                : "This is an automated message, please do not reply.";

            var rightsText = isArabic ? "جميع الحقوق محفوظة" : "All rights reserved";

            var currentYear = DateTime.UtcNow.Year;

            var logoHtml = !string.IsNullOrWhiteSpace(logoContentId)
                ? $@"
                <img
                  src='cid:{logoContentId}'
                  alt='Qatrat Hayat Logo'
                  width='125px'
                  height='auto'
                  style='
                    display: block;
                    margin: 0 auto 14px auto;
                    border: 0;
                    outline: none;
                    text-decoration: none;
                  '
                />"
                : "";

            return $@"
<!doctype html>
<html lang='{htmlLang}' dir='{direction}'>
  <head>
    <meta charset='UTF-8' />
    <meta name='viewport' content='width=device-width, initial-scale=1.0' />
    <title>{pageTitle}</title>
  </head>

  <body
    style='
      font-family: Cairo, Tahoma, Arial, sans-serif;
      margin: 0;
      padding: 0;
      box-sizing: border-box;
      outline: 0;
      border: none;
      text-decoration: none;
      direction: {direction};
      text-align: {textAlign};
      background-color: #ffffff;
    '
  >
    <table
      width='100%'
      cellpadding='0'
      cellspacing='0'
      border='0'
      style='width: 100%; background-color: #ffffff; padding: 32px 12px'
    >
      <tr>
        <td align='center'>
          <!-- Main Card -->
          <table
            width='600'
            cellpadding='0'
            cellspacing='0'
            border='0'
            style='
              width: 100%;
              max-width: 600px;
              background-color: #ffffff;
              border-radius: 18px;
              overflow: hidden;
              border: 1px solid #eeeeee;
              box-shadow: 0 0.5rem 1.5rem rgba(0, 0, 0, 0.18);
            '
          >
            <!-- Header -->
            <tr>
              <td
                style='
                  background-color: #07152d;
                  background: linear-gradient(
                    135deg,
                    #07152d 0%,
                    #0b1d3d 60%,
                    #10264b 100%
                  );
                  padding: 30px 24px;
                  text-align: center;
                  color: #ffffff;
                '
              >
                <!-- Logo -->
                {logoHtml}

                <h1
                  style='
                    margin: 0;
                    font-size: 26px;
                    line-height: 1.4;
                    font-weight: 700;
                    color: #ffffff;
                  '
                >
                  {appName}
                </h1>

                <p
                  style='
                    margin: 8px 0 0 0;
                    font-size: 14px;
                    line-height: 1.7;
                    color: #ffeff0;
                  '
                >
                  {headerSubtitle}
                </p>
              </td>
            </tr>

            <!-- Body -->
            <tr>
              <td
                style='
                  padding: 36px 34px;
                  color: #101727;
                  direction: {direction};
                  text-align: {textAlign};
                '
              >
                <h2
                  style='
                    margin: 0 0 18px 0;
                    font-size: 22px;
                    line-height: 1.5;
                    font-weight: 700;
                    color: #101727;
                  '
                >
                  {mainTitle}
                </h2>

                <p
                  style='
                    margin: 0 0 16px 0;
                    font-size: 16px;
                    line-height: 1.9;
                    color: #101727;
                  '
                >
                  {greeting}
                </p>

                <p
                  style='
                    margin: 0 0 26px 0;
                    font-size: 15px;
                    line-height: 1.9;
                    color: #333333;
                  '
                >
                  {introText}
                </p>

                <!-- OTP Box -->
                <table width='100%' cellpadding='0' cellspacing='0' border='0'>
                  <tr>
                    <td align='center' style='padding: 8px 0 30px 0'>
                      <div
                        style='
                          display: inline-block;
                          background-color: #ffeff0;
                          border: 1px solid #f0cccc;
                          border-radius: 14px;
                          padding: 20px 30px;
                        '
                      >
                        <span
                          style='
                            display: inline-block;
                            font-size: 36px;
                            line-height: 1;
                            letter-spacing: 8px;
                            font-weight: 700;
                            color: #bf0007;
                            direction: ltr;
                            text-align: center;
                          '
                        >
                          {otp}
                        </span>
                      </div>
                    </td>
                  </tr>
                </table>

                <p
                  style='
                    margin: 0 0 16px 0;
                    font-size: 15px;
                    line-height: 1.9;
                    color: #333333;
                  '
                >
                  {expiryText}
                  <strong style='color: #bf0007'>{expiryDuration}</strong>.
                </p>

                <!-- Security Note -->
                <div
                  style='
                    margin: 24px 0;
                    padding: 16px 18px;
                    background-color: #ffeff0;
                    border-{borderSide}: 4px solid #bf0007;
                    border-radius: 10px;
                  '
                >
                  <p
                    style='
                      margin: 0;
                      font-size: 14px;
                      line-height: 1.8;
                      color: #555555;
                    '
                  >
                    {securityNote}
                  </p>
                </div>

                <p
                  style='
                    margin: 26px 0 0 0;
                    font-size: 15px;
                    line-height: 1.8;
                    color: #101727;
                    font-weight: 600;
                  '
                >
                  {teamName}
                </p>
              </td>
            </tr>

            <!-- Footer -->
            <tr>
              <td
                style='
                  background-color: #ffffff;
                  padding: 20px 24px;
                  text-align: center;
                  border-top: 1px solid rgba(16, 23, 39, 0.08);
                '
              >
                <p
                  style='
                    margin: 0;
                    font-size: 12px;
                    line-height: 1.8;
                    color: #777777;
                  '
                >
                  {automaticMessage}
                </p>

                <p
                  style='
                    margin: 8px 0 0 0;
                    font-size: 12px;
                    line-height: 1.8;
                    color: #999999;
                  '
                >
                  © {currentYear} {appName}. {rightsText}.
                </p>
              </td>
            </tr>
          </table>
          <!-- End Main Card -->
        </td>
      </tr>
    </table>
  </body>
</html>";
        }
    }
}
