namespace QatratHayat.Application.Common.Interfaces
{
    public interface IEmailService
    {
        Task SendPasswordResetOtpAsync(
            string toEmail,
            string fullName,
            string otp,
            bool isArabic,
            CancellationToken cancellationToken = default
        );
    }
}