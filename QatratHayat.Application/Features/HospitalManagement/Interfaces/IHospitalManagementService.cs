using QatratHayat.Application.Common.DTOS;
using QatratHayat.Application.Features.HospitalManagement.DOTS;
using QatratHayat.Application.Features.HospitalManagement.DTOS;

namespace QatratHayat.Application.Features.HospitalManagement.Interfaces
{
    public interface IHospitalManagementService
    {
        Task<PagedResultDto<HospitalResponseDto>> GetAllHospitalsAsync(HospitalQueryDto query);

        Task<HospitalResponseDto> GetHospitalByIdAsync(int hospitalId);

        Task<HospitalResponseDto> AddHospitalAsync(AddHospitalRequestDto request);

        Task<HospitalResponseDto> UpdateHospitalAsync(int hospitalId, UpdateHospitalRequestDto request);

        Task SoftDeleteHospitalAsync(int hospitalId);

        Task ActivateHospitalAsync(int hospitalId);

        Task DeactivateHospitalAsync(int hospitalId);
    }
}