using QatratHayat.Application.Common.DTOS;

namespace QatratHayat.Application.Features.Accounts.DTOs
{
    // This class represents the data returned to the user after successful registration or login.
    public class LoginResponseDto : BaseUserInfoDto
    {
        public string Token { get; set; } = null!;
    }
}