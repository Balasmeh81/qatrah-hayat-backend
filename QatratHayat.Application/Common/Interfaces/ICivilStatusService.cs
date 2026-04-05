using QatratHayat.Application.Accounts.DTOs;

namespace QatratHayat.Application.Common.Interfaces
{
    public interface ICivilStatusService
    {
         Task<NationalRegistryResponseDto> GetNationalRegistryAsync(String NationalId);
    }
}
