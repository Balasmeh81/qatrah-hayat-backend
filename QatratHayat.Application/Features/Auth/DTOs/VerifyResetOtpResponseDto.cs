namespace QatratHayat.Application.Features.Auth.DTOs
{
    public class VerifyResetOtpResponseDto
    {
        public string ResetSessionToken { get; set; } = null!;
        public string Message { get; set; } = null!;
    }
}