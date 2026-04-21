using QatratHayat.Application.Common.DTOS;
using QatratHayat.Application.Features.UsersManagement.DTOS;

namespace QatratHayat.Application.Features.UsersManagement.Interfaces
{
    public interface IUsersManagementService
    {
        // Statistics
        Task<UsersStatisticsResponseDto> GetStatisticsAsync();
        // Staff Methods
        Task<StaffInfoResponseDto> GetStaffByIdAsync(int userId);

        Task<PagedResultDto<StaffInfoResponseDto>> GetAllStaffUsersAsync(UserManagementQueryDto query);

        Task<StaffInfoResponseDto> UpdateStaffAsync(int userId, UpdateStaffRequestDto dto);

        Task<CitizenResponseDto> LookupCitizenByNationalIdAsync(string nationalId);

        Task<StaffInfoResponseDto> CreateStaffFromNationalRegistryAsync(
            CreateStaffFromRegistryRequestDto dto
        );

        Task<StaffInfoResponseDto> PromoteCitizenToStaffAsync(
            int userId,
            PromoteCitizenToStaffRequestDto dto
        );


        // Citizen Methods
        Task<CitizenInfoResponseDto> GetCitizenByIdAsync(int userId);

        Task<PagedResultDto<CitizenInfoResponseDto>> GetAllCitizenUsersAsync(UserManagementQueryDto query);

        Task<CitizenInfoResponseDto> UpdateCitizenAsync(int userId, UpdateCitizenRequestDto dto);


        // Shared Methods
        Task ActivateUserAsync(int userId);

        Task DeactivateUserAsync(int userId);

        Task SoftDeleteUserAsync(int userId);
    }
}