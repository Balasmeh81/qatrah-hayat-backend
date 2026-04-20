using QatratHayat.Application.Common.DTOS;
using QatratHayat.Application.Features.UsersManagement.DTOS;

namespace QatratHayat.Application.Features.UsersManagement.Interfaces
{
    public interface IUsersManagementService
    {
        // Staff Methods
        Task<StaffInfoResponseDto> GetStaffByIdAsync(int userId);

        Task<PagedResultDto<StaffInfoResponseDto>> GetAllStaffUsersAsync(UserManagementQueryDto query);

        Task<StaffInfoResponseDto> AddStaffAsync(AddStaffRequestDto dto);

        Task<StaffInfoResponseDto> UpdateStaffAsync(int userId, UpdateStaffRequestDto dto);


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