namespace QatratHayat.Application.Accounts.DTOs
{
    // This class represents the data returned to the user after successful registration or login.
    public class AuthResponseDto
    {
        public int UserId { get; set; }
        public string Email { get; set; } = null!;
        public string FullNameAr { get; set; } = null!;
        public string FullNameEn { get; set; } = null!;
        public string Role { get; set; } = null!;
        public string Token { get; set; } = null!;
    }
}