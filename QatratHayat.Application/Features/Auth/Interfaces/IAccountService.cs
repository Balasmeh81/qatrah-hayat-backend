using QatratHayat.Application.Features.Accounts.DTOs;
using System.Security.Claims;


namespace QatratHayat.Application.Features.Auth.Interfaces
{
    public interface IAccountService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);
        Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
        Task<CurrentUserDto> GetCurrentUserAsync(ClaimsPrincipal userPrincipal);
    }
}
