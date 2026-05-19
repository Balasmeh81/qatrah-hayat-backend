using QatratHayat.Application.Common.DTOS;
using QatratHayat.Application.Features.Inventory.DTOs;

namespace QatratHayat.Application.Features.Inventory.Interfaces
{
    public interface IInventoryService
    {
        Task<PagedResultDto<BloodUnitListItemDto>> GetBloodUnitsAsync(
            int userId,
            BloodUnitQueryDto query
        );

        Task<BloodUnitDetailsDto> GetBloodUnitByIdAsync(
            int userId,
            int bloodUnitId
        );

        Task<InventoryStatisticsDto> GetStatisticsAsync(
            int userId,
            int? branchId
        );

        Task<int> MarkExpiredUnitsAsync(int userId);

        Task<BloodUnitDetailsDto> DisposeBloodUnitAsync(
            int userId,
            int bloodUnitId,
            DisposeBloodUnitRequestDto request
        );

        Task<BloodUnitDetailsDto> ReturnBloodUnitToAvailableAsync(
            int userId,
            int bloodUnitId,
            ReturnBloodUnitToAvailableRequestDto request
        );
    }
}