using QatratHayat.Application.Features.Accounts.DTOs;

namespace QatratHayat.Application.Features.Auth.Interfaces
{
    public interface ICivilStatusService
    {
         Task<NationalRegistryResponseDto> GetNationalRegistryAsync(string NationalId);
    }
}
