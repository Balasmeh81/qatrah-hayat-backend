using QatratHayat.Application.Accounts.DTOs;
using System.Security.Claims;


namespace QatratHayat.Application.Common.Interfaces
{
    public interface IAccountService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);
        Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
        Task<CurrentUserDto> GetCurrentUserAsync(ClaimsPrincipal userPrincipal);
    }
}
