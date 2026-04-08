namespace QatratHayat.Application.Features.Accounts.DTOs
{
    // This class represents the current logged-in user data.
    public class CurrentUserDto
    {
        public int UserId { get; set; }
        public string Email { get; set; } = null!;
        public string FullNameAr { get; set; } = null!;
        public string FullNameEn { get; set; } = null!;
        public string Role { get; set; } = null!;
    }
}