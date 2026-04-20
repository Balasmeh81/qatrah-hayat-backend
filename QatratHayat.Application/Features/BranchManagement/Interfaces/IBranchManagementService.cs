using QatratHayat.Application.Common.DTOS;
using QatratHayat.Application.Features.BranchManagement.DTOS;

namespace QatratHayat.Application.Features.BranchManagement.Interfaces
{
    public interface IBranchManagementService
    {
        Task<PagedResultDto<BranchResponseDto>> GetAllBranchesAsync(BranchQueryDto query);

        Task<BranchResponseDto> GetBranchByIdAsync(int branchId);

        Task<BranchResponseDto> AddBranchAsync(AddBranchRequestDto request);

        Task<BranchResponseDto> UpdateBranchAsync(int branchId, UpdateBranchRequestDto request);

        Task SoftDeleteBranchAsync(int branchId);

        Task ActivateBranchAsync(int branchId);

        Task DeactivateBranchAsync(int branchId);
    }
}