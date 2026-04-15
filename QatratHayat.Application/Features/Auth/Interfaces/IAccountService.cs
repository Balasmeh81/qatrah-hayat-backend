using QatratHayat.Application.Features.Accounts.DTOs;
using QatratHayat.Application.Features.Auth.DTOs;
using System.Security.Claims;


namespace QatratHayat.Application.Features.Auth.Interfaces
{
    public interface IAccountService
    {
        Task<RegisterResponseDto> RegisterCitizenAsync(RegisterRequestDto request);
        Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
        Task<CurrentUserDto> GetCurrentUserAsync(ClaimsPrincipal userPrincipal);
    }
}
