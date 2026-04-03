namespace QatratHayat.Application.Accounts.DTOs
{
    public class LoginRequestDto
    {
        public string EmailOrNationalId { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}