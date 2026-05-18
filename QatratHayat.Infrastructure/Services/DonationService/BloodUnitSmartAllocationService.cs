using Microsoft.EntityFrameworkCore;
using QatratHayat.Application.Features.Donations.Interfaces;
using QatratHayat.Domain.Entities;
using QatratHayat.Domain.Enums;
using QatratHayat.Infrastructure.Persistence;

namespace QatratHayat.Infrastructure.Services
{
    public class BloodUnitSmartAllocationService : IBloodUnitSmartAllocationService
    {
        private readonly AppDbContext _context;
        private readonly IBloodTypeCompatibilityService _bloodTypeCompatibilityService;

        public BloodUnitSmartAllocationService(
            AppDbContext context,
            IBloodTypeCompatibilityService bloodTypeCompatibilityService
        )
        {
            _context = context;
            _bloodTypeCompatibilityService = bloodTypeCompatibilityService;
        }

        public async Task AllocateBloodUnitAsync(BloodUnit bloodUnit, Donation donation)
        {
            // Priority 1:
            // Try original blood request if this was request-based donation.
            if (donation.BloodRequestId.HasValue)
            {
                var originalRequest = await GetEligibleBloodRequestAsync(
                    donation.BloodRequestId.Value,
                    bloodUnit.BranchId
                );

                if (
                    originalRequest is not null
                    && IsBloodUnitCompatibleWithRequest(bloodUnit, originalRequest)
                    && GetUnitsRemaining(originalRequest) > 0
                )
                {
                    AllocateUnitToRequest(bloodUnit, originalRequest);
                    return;
                }
            }

            // Priority 2:
            // Search another compatible published request in the same branch.
            var compatibleRequest = await FindBestCompatibleRequestInSameBranchAsync(bloodUnit);

            if (compatibleRequest is not null)
            {
                AllocateUnitToRequest(bloodUnit, compatibleRequest);
                return;
            }

            // Priority 3:
            // Campaign matching is intentionally left for Campaign Feature implementation.
            // Current project state: Campaign entity exists, but Campaign workflow is not fully implemented.

            // Priority 4:
            // Keep unit in inventory as Available.
            KeepUnitAvailable(bloodUnit);
        }

        private async Task<BloodRequest?> GetEligibleBloodRequestAsync(int requestId, int branchId)
        {
            return await _context.BloodRequests
                .Include(x => x.BloodUnits)
                .FirstOrDefaultAsync(x =>
                    x.Id == requestId
                    && x.BranchId == branchId
                    && x.PublishedAt.HasValue
                    && x.BloodType.HasValue
                    && x.UnitsNeeded.HasValue
                    && (
                        x.RequestStatus == RequestStatus.Shortage
                        || x.RequestStatus == RequestStatus.PartiallyAllocated
                    )
                );
        }

        private async Task<BloodRequest?> FindBestCompatibleRequestInSameBranchAsync(
            BloodUnit bloodUnit
        )
        {
            var requests = await _context.BloodRequests
                .Include(x => x.BloodUnits)
                .Where(x =>
                    x.BranchId == bloodUnit.BranchId
                    && x.PublishedAt.HasValue
                    && x.BloodType.HasValue
                    && x.UnitsNeeded.HasValue
                    && (
                        x.RequestStatus == RequestStatus.Shortage
                        || x.RequestStatus == RequestStatus.PartiallyAllocated
                    )
                )
                .ToListAsync();

            return requests
                .Where(x =>
                    GetUnitsRemaining(x) > 0
                    && IsBloodUnitCompatibleWithRequest(bloodUnit, x)
                )
                .OrderByDescending(x => x.UrgencyLevel == UrgencyLevel.Emergency)
                .ThenBy(x => x.PublishedAt)
                .FirstOrDefault();
        }

        private bool IsBloodUnitCompatibleWithRequest(BloodUnit bloodUnit, BloodRequest request)
        {
            if (!request.BloodType.HasValue)
            {
                return false;
            }

            return _bloodTypeCompatibilityService.CanDonateTo(
                bloodUnit.BloodType,
                request.BloodType.Value
            );
        }

        private static int GetUnitsRemaining(BloodRequest request)
        {
            if (!request.UnitsNeeded.HasValue)
            {
                return 0;
            }

            var allocatedOrUsedUnits = request.BloodUnits.Count(unit =>
                unit.UnitStatus == UnitStatus.PartiallyAllocated
                || unit.UnitStatus == UnitStatus.Allocated
                || unit.UnitStatus == UnitStatus.Used
            );

            return Math.Max(request.UnitsNeeded.Value - allocatedOrUsedUnits, 0);
        }

        private static void AllocateUnitToRequest(BloodUnit bloodUnit, BloodRequest request)
        {
            var unitsRemainingBeforeAllocation = GetUnitsRemaining(request);

            bloodUnit.AllocatedToRequestId = request.Id;
            bloodUnit.AllocatedAt = DateTime.UtcNow;
            bloodUnit.UpdatedAt = DateTime.UtcNow;

            if (unitsRemainingBeforeAllocation <= 1)
            {
                bloodUnit.UnitStatus = UnitStatus.Allocated;
                request.RequestStatus = RequestStatus.Processing;
            }
            else
            {
                bloodUnit.UnitStatus = UnitStatus.PartiallyAllocated;
                request.RequestStatus = RequestStatus.PartiallyAllocated;
            }

            request.UpdatedAt = DateTime.UtcNow;
        }

        private static void KeepUnitAvailable(BloodUnit bloodUnit)
        {
            bloodUnit.AllocatedToRequestId = null;
            bloodUnit.AllocatedAt = null;
            bloodUnit.UnitStatus = UnitStatus.Available;
            bloodUnit.UpdatedAt = DateTime.UtcNow;
        }
    }
}