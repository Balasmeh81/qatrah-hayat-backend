using QatratHayat.Application.Features.Auth.DTOs;

namespace QatratHayat.Application.Features.Accounts.DTOs
{
    // This class represents the data returned to the user after successful registration or login.
    public class LoginResponseDto : UserInfoDto
    {
        public string Token { get; set; } = null!;
    }
}