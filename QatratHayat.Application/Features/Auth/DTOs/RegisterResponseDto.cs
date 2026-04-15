namespace QatratHayat.Application.Features.Auth.DTOs
{
    public class RegisterResponseDto
    {
        public int UserId { get; set; }
        public string Email { get; set; } = null!;
        public string FullNameAr { get; set; } = null!;
        public string FullNameEn { get; set; } = null!;
        public string Message { get; set; } = null!;
    }
}
