using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QatratHayat.Application.Common.DTOS;
using QatratHayat.Application.Common.Exceptions;
using QatratHayat.Application.Features.Inventory.DTOs;
using QatratHayat.Application.Features.Inventory.Interfaces;
using QatratHayat.Domain.Entities;
using QatratHayat.Domain.Enums;
using QatratHayat.Infrastructure.Identity;
using QatratHayat.Infrastructure.Persistence;

namespace QatratHayat.Infrastructure.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        private const int ExpiringSoonDays = 7;

        public InventoryService(
            AppDbContext context,
            UserManager<ApplicationUser> userManager
        )
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<PagedResultDto<BloodUnitListItemDto>> GetBloodUnitsAsync(
            int userId,
            BloodUnitQueryDto query
        )
        {
            NormalizePaging(query);

            var inventoryScope = await GetInventoryScopeAsync(
                userId,
                allowAdmin: true
            );

            var bloodUnitsQuery = GetBloodUnitBaseQuery();

            bloodUnitsQuery = ApplyBranchScope(
                bloodUnitsQuery,
                inventoryScope,
                query.BranchId
            );

            bloodUnitsQuery = ApplyFilters(bloodUnitsQuery, query);

            var totalCount = await bloodUnitsQuery.CountAsync();

            var units = await bloodUnitsQuery
                .OrderByDescending(unit => unit.CreatedAt)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(unit => MapToListItemDto(unit))
                .ToListAsync();

            return new PagedResultDto<BloodUnitListItemDto>
            {
                Items = units,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                TotalCount = totalCount
            };
        }

        public async Task<BloodUnitDetailsDto> GetBloodUnitByIdAsync(
            int userId,
            int bloodUnitId
        )
        {
            var inventoryScope = await GetInventoryScopeAsync(
                userId,
                allowAdmin: true
            );

            var unit = await GetBloodUnitBaseQuery()
                .FirstOrDefaultAsync(unit => unit.Id == bloodUnitId);

            if (unit is null)
            {
                throw new NotFoundException(
                    "Blood unit was not found.",
                    ErrorCodes.BloodUnitNotFound
                );
            }

            EnsureUnitInsideUserScope(unit, inventoryScope);

            return MapToDetailsDto(unit);
        }

        public async Task<InventoryStatisticsDto> GetStatisticsAsync(
            int userId,
            int? branchId
        )
        {
            var inventoryScope = await GetInventoryScopeAsync(
                userId,
                allowAdmin: true
            );

            var query = _context.BloodUnits.AsNoTracking().AsQueryable();

            query = ApplyBranchScope(query, inventoryScope, branchId);

            var now = DateTime.UtcNow;
            var expiringSoonLimit = now.AddDays(ExpiringSoonDays);

            var units = await query
                .Select(unit => new
                {
                    unit.BloodType,
                    unit.UnitStatus,
                    unit.ExpiresAt
                })
                .ToListAsync();

            var bloodTypeSummaries = Enum
                .GetValues<BloodType>()
                .Select(bloodType =>
                {
                    var matchingUnits = units
                        .Where(unit => unit.BloodType == bloodType)
                        .ToList();

                    return new BloodTypeInventorySummaryDto
                    {
                        BloodType = bloodType,
                        BloodTypeDisplayName = bloodType.ToDisplayName(),

                        AvailableCount = matchingUnits.Count(unit =>
                            unit.UnitStatus == UnitStatus.Available
                            && unit.ExpiresAt > now
                        ),

                        PartiallyAllocatedCount = matchingUnits.Count(unit =>
                            unit.UnitStatus == UnitStatus.PartiallyAllocated
                        ),

                        AllocatedCount = matchingUnits.Count(unit =>
                            unit.UnitStatus == UnitStatus.Allocated
                        ),

                        UsedCount = matchingUnits.Count(unit =>
                            unit.UnitStatus == UnitStatus.Used
                        ),

                        ExpiredCount = matchingUnits.Count(unit =>
                            unit.UnitStatus == UnitStatus.Expired
                            || (
                                unit.ExpiresAt <= now
                                && unit.UnitStatus != UnitStatus.Disposed
                                && unit.UnitStatus != UnitStatus.Used
                            )
                        ),

                        DisposedCount = matchingUnits.Count(unit =>
                            unit.UnitStatus == UnitStatus.Disposed
                        ),

                        ExpiringSoonCount = matchingUnits.Count(unit =>
                            unit.UnitStatus == UnitStatus.Available
                            && unit.ExpiresAt > now
                            && unit.ExpiresAt <= expiringSoonLimit
                        ),

                        TotalCount = matchingUnits.Count
                    };
                })
                .ToList();

            return new InventoryStatisticsDto
            {
                TotalUnits = units.Count,

                AvailableUnits = units.Count(unit =>
                    unit.UnitStatus == UnitStatus.Available
                    && unit.ExpiresAt > now
                ),

                ReservedUnits = units.Count(unit =>
                    unit.UnitStatus == UnitStatus.PartiallyAllocated
                ),

                AllocatedUnits = units.Count(unit =>
                    unit.UnitStatus == UnitStatus.Allocated
                ),

                UsedUnits = units.Count(unit =>
                    unit.UnitStatus == UnitStatus.Used
                ),

                ExpiredUnits = units.Count(unit =>
                    unit.UnitStatus == UnitStatus.Expired
                    || (
                        unit.ExpiresAt <= now
                        && unit.UnitStatus != UnitStatus.Disposed
                        && unit.UnitStatus != UnitStatus.Used
                    )
                ),

                DisposedUnits = units.Count(unit =>
                    unit.UnitStatus == UnitStatus.Disposed
                ),

                ExpiringSoonUnits = units.Count(unit =>
                    unit.UnitStatus == UnitStatus.Available
                    && unit.ExpiresAt > now
                    && unit.ExpiresAt <= expiringSoonLimit
                ),

                BloodTypes = bloodTypeSummaries
            };
        }

        public async Task<int> MarkExpiredUnitsAsync(int userId)
        {
            var inventoryScope = await GetInventoryScopeAsync(
                userId,
                allowAdmin: true
            );

            var now = DateTime.UtcNow;

            var query = _context.BloodUnits
                .Where(unit =>
                    unit.ExpiresAt <= now
                    && unit.UnitStatus != UnitStatus.Expired
                    && unit.UnitStatus != UnitStatus.Disposed
                    && unit.UnitStatus != UnitStatus.Used
                );

            query = ApplyBranchScope(query, inventoryScope, branchId: null);

            var expiredUnits = await query.ToListAsync();

            foreach (var unit in expiredUnits)
            {
                unit.UnitStatus = UnitStatus.Expired;
                unit.UpdatedAt = now;
            }

            await _context.SaveChangesAsync();

            return expiredUnits.Count;
        }

        public async Task<BloodUnitDetailsDto> DisposeBloodUnitAsync(
            int userId,
            int bloodUnitId,
            DisposeBloodUnitRequestDto request
        )
        {
            if (request is null)
            {
                throw new BadRequestException(
                    "Request body is required.",
                    ErrorCodes.BadRequest
                );
            }

            if (string.IsNullOrWhiteSpace(request.DisposalReason))
            {
                throw new BadRequestException(
                    "Disposal reason is required.",
                    ErrorCodes.BloodUnitDisposalReasonRequired
                );
            }

            var inventoryScope = await GetInventoryScopeAsync(
                userId,
                allowAdmin: false
            );

            var unit = await GetBloodUnitBaseQuery()
                .FirstOrDefaultAsync(unit => unit.Id == bloodUnitId);

            if (unit is null)
            {
                throw new NotFoundException(
                    "Blood unit was not found.",
                    ErrorCodes.BloodUnitNotFound
                );
            }

            EnsureUnitInsideUserScope(unit, inventoryScope);

            if (
                unit.UnitStatus == UnitStatus.Disposed
                || unit.UnitStatus == UnitStatus.Used
            )
            {
                throw new BadRequestException(
                    "This blood unit cannot be disposed in its current status.",
                    ErrorCodes.InvalidBloodUnitStatus
                );
            }

            if (
                unit.UnitStatus == UnitStatus.Allocated
                || unit.UnitStatus == UnitStatus.PartiallyAllocated
            )
            {
                throw new BadRequestException(
                    "Allocated or reserved blood units cannot be disposed before releasing the allocation.",
                    ErrorCodes.BloodUnitAllocationMustBeReleasedFirst
                );
            }

            var now = DateTime.UtcNow;

            unit.UnitStatus = UnitStatus.Disposed;
            unit.DisposalReason = request.DisposalReason.Trim();
            unit.DisposalDate = now;
            unit.DisposedByEmployeeId = userId;
            unit.UpdatedAt = now;

            await _context.SaveChangesAsync();

            return MapToDetailsDto(unit);
        }

        public async Task<BloodUnitDetailsDto> ReturnBloodUnitToAvailableAsync(
            int userId,
            int bloodUnitId,
            ReturnBloodUnitToAvailableRequestDto request
        )
        {
            if (request is null)
            {
                throw new BadRequestException(
                    "Request body is required.",
                    ErrorCodes.BadRequest
                );
            }

            if (string.IsNullOrWhiteSpace(request.DeallocationNote))
            {
                throw new BadRequestException(
                    "Deallocation note is required.",
                    ErrorCodes.BloodUnitDeallocationNoteRequired
                );
            }

            var inventoryScope = await GetInventoryScopeAsync(
                userId,
                allowAdmin: false
            );

            var unit = await GetBloodUnitBaseQuery()
                .FirstOrDefaultAsync(unit => unit.Id == bloodUnitId);

            if (unit is null)
            {
                throw new NotFoundException(
                    "Blood unit was not found.",
                    ErrorCodes.BloodUnitNotFound
                );
            }

            EnsureUnitInsideUserScope(unit, inventoryScope);

            if (unit.UnitStatus != UnitStatus.PartiallyAllocated)
            {
                throw new BadRequestException(
                    "Only temporarily reserved blood units can be returned to available.",
                    ErrorCodes.InvalidBloodUnitStatus
                );
            }

            if (unit.ExpiresAt <= DateTime.UtcNow)
            {
                unit.UnitStatus = UnitStatus.Expired;
                unit.AllocatedToRequestId = null;
                unit.AllocatedAt = null;
                unit.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                throw new BadRequestException(
                    "This blood unit is expired and cannot be returned to available.",
                    ErrorCodes.BloodUnitExpired
                );
            }

            var now = DateTime.UtcNow;
            var requestId = unit.AllocatedToRequestId;

            unit.UnitStatus = UnitStatus.Available;
            unit.AllocatedToRequestId = null;
            unit.AllocatedAt = null;
            unit.DeallocationNote = request.DeallocationNote.Trim();
            unit.UpdatedAt = now;

            if (requestId.HasValue)
            {
                await RecalculateBloodRequestStatusAsync(requestId.Value);
            }

            await _context.SaveChangesAsync();

            return MapToDetailsDto(unit);
        }

        // ============================================================
        // Query Helpers
        // ============================================================

        private IQueryable<BloodUnit> GetBloodUnitBaseQuery()
        {
            return _context
                .BloodUnits
                .Include(unit => unit.Branch)
                .Include(unit => unit.Donation)
                .AsQueryable();
        }

        private static IQueryable<BloodUnit> ApplyFilters(
            IQueryable<BloodUnit> query,
            BloodUnitQueryDto filter
        )
        {
            var now = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var search = filter.SearchTerm.Trim();

                query = query.Where(unit =>
                    unit.UnitCode.Contains(search)
                    || unit.Branch.BranchNameAr.Contains(search)
                    || unit.Branch.BranchNameEn.Contains(search)
                );
            }

            if (filter.BloodType.HasValue)
            {
                query = query.Where(unit =>
                    unit.BloodType == filter.BloodType.Value
                );
            }

            if (filter.UnitStatus.HasValue)
            {
                query = query.Where(unit =>
                    unit.UnitStatus == filter.UnitStatus.Value
                );
            }

            if (filter.FromCollectionDate.HasValue)
            {
                query = query.Where(unit =>
                    unit.CollectionDate >= filter.FromCollectionDate.Value
                );
            }

            if (filter.ToCollectionDate.HasValue)
            {
                query = query.Where(unit =>
                    unit.CollectionDate <= filter.ToCollectionDate.Value
                );
            }

            if (filter.ExpiringBefore.HasValue)
            {
                query = query.Where(unit =>
                    unit.ExpiresAt <= filter.ExpiringBefore.Value
                );
            }

            if (filter.ExpiredOnly == true)
            {
                query = query.Where(unit =>
                    unit.ExpiresAt <= now
                    || unit.UnitStatus == UnitStatus.Expired
                );
            }

            return query;
        }

        private static IQueryable<BloodUnit> ApplyBranchScope(
            IQueryable<BloodUnit> query,
            InventoryScope scope,
            int? branchId
        )
        {
            if (scope.IsAdmin)
            {
                return branchId.HasValue
                    ? query.Where(unit => unit.BranchId == branchId.Value)
                    : query;
            }

            return query.Where(unit => unit.BranchId == scope.BranchId);
        }

        private static void NormalizePaging(BloodUnitQueryDto query)
        {
            if (query.PageNumber < 1)
            {
                query.PageNumber = 1;
            }

            if (query.PageSize < 1)
            {
                query.PageSize = 10;
            }

            if (query.PageSize > 100)
            {
                query.PageSize = 100;
            }
        }

        // ============================================================
        // Authorization Helpers
        // ============================================================

        private async Task<InventoryScope> GetInventoryScopeAsync(
            int userId,
            bool allowAdmin
        )
        {
            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.Id == userId
                && u.IsActive
                && !u.IsDeleted
            );

            if (user is null)
            {
                throw new NotFoundException(
                    "Current user was not found.",
                    ErrorCodes.CurrentUserNotFound
                );
            }

            var roles = await _userManager.GetRolesAsync(user);

            var isAdmin = roles.Contains(UserRole.Admin.ToString());
            var isEmployee = roles.Contains(UserRole.Employee.ToString());
            var isBranchManager = roles.Contains(UserRole.BranchManager.ToString());

            if (isAdmin && allowAdmin)
            {
                return new InventoryScope
                {
                    IsAdmin = true,
                    BranchId = null
                };
            }

            if (!isEmployee && !isBranchManager)
            {
                throw new BadRequestException(
                    "Only employee or branch manager can perform this inventory action.",
                    ErrorCodes.InventoryActionNotAllowed
                );
            }

            if (isEmployee)
            {
                if (!user.BranchId.HasValue)
                {
                    throw new BadRequestException(
                        "Employee is not assigned to a branch.",
                        ErrorCodes.EmployeeBranchNotAssigned
                    );
                }

                return new InventoryScope
                {
                    IsAdmin = false,
                    BranchId = user.BranchId.Value
                };
            }

            var branch = await _context.Branches.FirstOrDefaultAsync(branch =>
                branch.ManagerUserId == user.Id
                && branch.IsActive
                && !branch.IsDeleted
            );

            if (branch is null)
            {
                throw new BadRequestException(
                    "Branch manager is not assigned to an active branch.",
                    ErrorCodes.EmployeeBranchNotAssigned
                );
            }

            return new InventoryScope
            {
                IsAdmin = false,
                BranchId = branch.Id
            };
        }

        private static void EnsureUnitInsideUserScope(
            BloodUnit unit,
            InventoryScope scope
        )
        {
            if (scope.IsAdmin)
            {
                return;
            }

            if (unit.BranchId != scope.BranchId)
            {
                throw new BadRequestException(
                    "This blood unit does not belong to your branch.",
                    ErrorCodes.BloodUnitBranchMismatch
                );
            }
        }

        // ============================================================
        // Status Helpers
        // ============================================================

        private async Task RecalculateBloodRequestStatusAsync(int requestId)
        {
            var request = await _context
                .BloodRequests
                .Include(request => request.BloodUnits)
                .FirstOrDefaultAsync(request => request.Id == requestId);

            if (request is null)
            {
                return;
            }

            if (
                request.RequestStatus == RequestStatus.Fulfilled
                || request.RequestStatus == RequestStatus.Cancelled
                || request.RequestStatus == RequestStatus.Rejected
            )
            {
                return;
            }

            var reservedOrAllocatedOrUsedCount = request.BloodUnits.Count(unit =>
                unit.UnitStatus == UnitStatus.PartiallyAllocated
                || unit.UnitStatus == UnitStatus.Allocated
                || unit.UnitStatus == UnitStatus.Used
            );

            var unitsNeeded = request.UnitsNeeded ?? 0;

            if (reservedOrAllocatedOrUsedCount <= 0)
            {
                request.RequestStatus = RequestStatus.Shortage;
            }
            else if (reservedOrAllocatedOrUsedCount < unitsNeeded)
            {
                request.RequestStatus = RequestStatus.PartiallyAllocated;
            }
            else
            {
                request.RequestStatus = RequestStatus.Processing;
            }

            request.UpdatedAt = DateTime.UtcNow;
        }

        // ============================================================
        // Mapping Helpers
        // ============================================================

        private static BloodUnitListItemDto MapToListItemDto(BloodUnit unit)
        {
            var now = DateTime.UtcNow;

            return new BloodUnitListItemDto
            {
                Id = unit.Id,
                UnitCode = unit.UnitCode,

                BloodType = unit.BloodType,
                BloodTypeDisplayName = unit.BloodType.ToDisplayName(),

                UnitStatus = unit.UnitStatus,

                CollectionDate = unit.CollectionDate,
                ExpiresAt = unit.ExpiresAt,
                CreatedAt = unit.CreatedAt,
                AllocatedAt = unit.AllocatedAt,

                BranchId = unit.BranchId,
                BranchNameAr = unit.Branch.BranchNameAr,
                BranchNameEn = unit.Branch.BranchNameEn,

                DonationId = unit.DonationId,
                AllocatedToRequestId = unit.AllocatedToRequestId,

                IsExpiredByDate = unit.ExpiresAt <= now,

                DaysUntilExpiry = (int)Math.Ceiling(
                    (unit.ExpiresAt.Date - now.Date).TotalDays
                )
            };
        }

        private static BloodUnitDetailsDto MapToDetailsDto(BloodUnit unit)
        {
            var now = DateTime.UtcNow;

            return new BloodUnitDetailsDto
            {
                Id = unit.Id,
                UnitCode = unit.UnitCode,

                BloodType = unit.BloodType,
                BloodTypeDisplayName = unit.BloodType.ToDisplayName(),

                UnitStatus = unit.UnitStatus,

                CollectionDate = unit.CollectionDate,
                ExpiresAt = unit.ExpiresAt,
                CreatedAt = unit.CreatedAt,
                UpdatedAt = unit.UpdatedAt,

                AllocatedAt = unit.AllocatedAt,
                DisposalDate = unit.DisposalDate,

                DisposalReason = unit.DisposalReason,
                DeallocationNote = unit.DeallocationNote,

                BranchId = unit.BranchId,
                BranchNameAr = unit.Branch.BranchNameAr,
                BranchNameEn = unit.Branch.BranchNameEn,

                DonationId = unit.DonationId,
                DonationType = unit.Donation.DonationType,
                DonorProfileId = unit.Donation.DonorProfileId,
                EmployeeUserId = unit.Donation.EmployeeUserId,

                AllocatedToRequestId = unit.AllocatedToRequestId,
                DisposedByEmployeeId = unit.DisposedByEmployeeId,

                IsExpiredByDate = unit.ExpiresAt <= now,

                DaysUntilExpiry = (int)Math.Ceiling(
                    (unit.ExpiresAt.Date - now.Date).TotalDays
                )
            };
        }

        private sealed class InventoryScope
        {
            public bool IsAdmin { get; set; }

            public int? BranchId { get; set; }
        }
    }
}