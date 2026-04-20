using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QatratHayat.Application.Common.DTOS;
using QatratHayat.Application.Common.Exceptions;
using QatratHayat.Application.Features.BranchManagement.DTOS;
using QatratHayat.Application.Features.BranchManagement.Interfaces;
using QatratHayat.Domain.Entities;
using QatratHayat.Domain.Enums;
using QatratHayat.Infrastructure.Identity;
using QatratHayat.Infrastructure.Persistence;

namespace QatratHayat.Application.Features.BranchManagement.Services
{
    public class BranchManagementService : IBranchManagementService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public BranchManagementService(
            AppDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ============================================================
        // Get All Branches
        // ============================================================

        public async Task<PagedResultDto<BranchResponseDto>> GetAllBranchesAsync(BranchQueryDto query)
        {
            NormalizePaging(query);

            var branchesQuery = _context.Branches
                .AsNoTracking()
                .Where(b => !b.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            {
                var searchTerm = query.SearchTerm.Trim();

                branchesQuery = branchesQuery.Where(b =>
                    b.BranchNameAr.Contains(searchTerm) ||
                    b.BranchNameEn.Contains(searchTerm) ||
                    b.AddressAr.Contains(searchTerm) ||
                    b.AddressEn.Contains(searchTerm) ||
                    (b.Email != null && b.Email.Contains(searchTerm)) ||
                    (b.Phone != null && b.Phone.Contains(searchTerm)));
            }

            if (query.IsActive.HasValue)
            {
                branchesQuery = branchesQuery.Where(b => b.IsActive == query.IsActive.Value);
            }

            var totalCount = await branchesQuery.CountAsync();

            var branches = await branchesQuery
                .OrderByDescending(b => b.CreatedAt)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            var items = await MapBranchesToDtosAsync(branches);

            return new PagedResultDto<BranchResponseDto>
            {
                Items = items,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                TotalCount = totalCount
            };
        }

        // ============================================================
        // Get Branch By Id
        // ============================================================

        public async Task<BranchResponseDto> GetBranchByIdAsync(int branchId)
        {
            var branch = await _context.Branches
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == branchId && !b.IsDeleted);

            if (branch is null)
            {
                throw new NotFoundException(
                    "Branch was not found.",
                    ErrorCodes.BranchNotFound
                );
            }

            return await MapBranchToDtoAsync(branch);
        }

        // ============================================================
        // Add Branch
        // ============================================================

        public async Task<BranchResponseDto> AddBranchAsync(AddBranchRequestDto request)
        {
            await ValidateBranchNameUniquenessAsync(
                request.BranchNameAr,
                request.BranchNameEn
            );

            var manager = await ValidateBranchManagerAsync(
                request.ManagerUserId,
                excludedBranchId: null
            );

            using var transaction = await _context.Database.BeginTransactionAsync();

            var branch = new Branch
            {
                BranchNameAr = request.BranchNameAr.Trim(),
                BranchNameEn = request.BranchNameEn.Trim(),
                AddressAr = request.AddressAr.Trim(),
                AddressEn = request.AddressEn.Trim(),
                ManagerUserId = request.ManagerUserId,
                GPSLat = request.GPSLat,
                GPSLng = request.GPSLng,
                Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim(),
                Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim(),
                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            };

            _context.Branches.Add(branch);

            await _context.SaveChangesAsync();

            manager.BranchId = branch.Id;
            manager.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            var createdBranch = await _context.Branches
                .AsNoTracking()
                .FirstAsync(b => b.Id == branch.Id);

            return await MapBranchToDtoAsync(createdBranch);
        }

        // ============================================================
        // Update Branch
        // ============================================================

        public async Task<BranchResponseDto> UpdateBranchAsync(
            int branchId,
            UpdateBranchRequestDto request)
        {
            var branch = await _context.Branches
                .FirstOrDefaultAsync(b => b.Id == branchId && !b.IsDeleted);

            if (branch is null)
            {
                throw new NotFoundException(
                    "Branch was not found.",
                    ErrorCodes.BranchNotFound
                );
            }

            await ValidateBranchNameUniquenessAsync(
                request.BranchNameAr,
                request.BranchNameEn,
                excludedBranchId: branchId
            );

            var newManager = await ValidateBranchManagerAsync(
                request.ManagerUserId,
                excludedBranchId: branchId
            );

            using var transaction = await _context.Database.BeginTransactionAsync();

            var oldManagerUserId = branch.ManagerUserId;

            branch.BranchNameAr = request.BranchNameAr.Trim();
            branch.BranchNameEn = request.BranchNameEn.Trim();
            branch.AddressAr = request.AddressAr.Trim();
            branch.AddressEn = request.AddressEn.Trim();
            branch.ManagerUserId = request.ManagerUserId;
            branch.GPSLat = request.GPSLat;
            branch.GPSLng = request.GPSLng;
            branch.Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();
            branch.Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim();
            branch.IsActive = request.IsActive;
            branch.UpdatedAt = DateTime.UtcNow;

            if (oldManagerUserId != request.ManagerUserId)
            {
                var oldManager = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == oldManagerUserId);

                if (oldManager is not null && oldManager.BranchId == branch.Id)
                {
                    oldManager.BranchId = null;
                    oldManager.UpdatedAt = DateTime.UtcNow;
                }
            }

            newManager.BranchId = branch.Id;
            newManager.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            var updatedBranch = await _context.Branches
                .AsNoTracking()
                .FirstAsync(b => b.Id == branch.Id);

            return await MapBranchToDtoAsync(updatedBranch);
        }

        // ============================================================
        // Soft Delete Branch
        // ============================================================

        public async Task SoftDeleteBranchAsync(int branchId)
        {
            var branch = await _context.Branches
                .FirstOrDefaultAsync(b => b.Id == branchId && !b.IsDeleted);

            if (branch is null)
            {
                throw new NotFoundException(
                    "Branch was not found.",
                    ErrorCodes.BranchNotFound
                );
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            branch.IsDeleted = true;
            branch.IsActive = false;
            branch.UpdatedAt = DateTime.UtcNow;

            var manager = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == branch.ManagerUserId);

            if (manager is not null && manager.BranchId == branch.Id)
            {
                manager.BranchId = null;
                manager.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
        }

        // ============================================================
        // Activate Branch
        // ============================================================

        public async Task ActivateBranchAsync(int branchId)
        {
            var branch = await _context.Branches
                .FirstOrDefaultAsync(b => b.Id == branchId && !b.IsDeleted);

            if (branch is null)
            {
                throw new NotFoundException(
                    "Branch was not found.",
                    ErrorCodes.BranchNotFound
                );
            }

            branch.IsActive = true;
            branch.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        // ============================================================
        // Deactivate Branch
        // ============================================================

        public async Task DeactivateBranchAsync(int branchId)
        {
            var branch = await _context.Branches
                .FirstOrDefaultAsync(b => b.Id == branchId && !b.IsDeleted);

            if (branch is null)
            {
                throw new NotFoundException(
                    "Branch was not found.",
                    ErrorCodes.BranchNotFound
                );
            }

            branch.IsActive = false;
            branch.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        // ============================================================
        // Validation Helpers
        // ============================================================

        private async Task<ApplicationUser> ValidateBranchManagerAsync(
            int managerUserId,
            int? excludedBranchId)
        {
            var manager = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.Id == managerUserId &&
                    u.IsActive &&
                    !u.IsDeleted);

            if (manager is null)
            {
                throw new NotFoundException(
                    "Branch manager user was not found.",
                    ErrorCodes.BranchManagerNotFound
                );
            }

            var roles = await _userManager.GetRolesAsync(manager);

            if (!roles.Contains(UserRole.BranchManager.ToString()))
            {
                throw new BadRequestException(
                    "Selected user is not a branch manager.",
                    ErrorCodes.UserIsNotBranchManager
                );
            }

            var isManagerAssignedToAnotherBranch = await _context.Branches
                .AnyAsync(b =>
                    b.ManagerUserId == managerUserId &&
                    !b.IsDeleted &&
                    (!excludedBranchId.HasValue || b.Id != excludedBranchId.Value));

            if (isManagerAssignedToAnotherBranch)
            {
                throw new ConflictException(
                    "This branch manager is already assigned to another branch.",
                    ErrorCodes.BranchManagerAlreadyAssigned
                );
            }

            return manager;
        }

        private async Task ValidateBranchNameUniquenessAsync(
            string branchNameAr,
            string branchNameEn,
            int? excludedBranchId = null)
        {
            var normalizedNameAr = branchNameAr.Trim();
            var normalizedNameEn = branchNameEn.Trim();

            var branchExists = await _context.Branches
                .AnyAsync(b =>
                    !b.IsDeleted &&
                    (b.BranchNameAr == normalizedNameAr ||
                     b.BranchNameEn == normalizedNameEn) &&
                    (!excludedBranchId.HasValue || b.Id != excludedBranchId.Value));

            if (branchExists)
            {
                throw new ConflictException(
                    "A branch with the same Arabic or English name already exists.",
                    ErrorCodes.BranchAlreadyExists
                );
            }
        }

        private static void NormalizePaging(BranchQueryDto query)
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
        // Mapping Helpers
        // ============================================================

        private async Task<BranchResponseDto> MapBranchToDtoAsync(Branch branch)
        {
            var manager = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == branch.ManagerUserId);

            return new BranchResponseDto
            {
                Id = branch.Id,
                BranchNameAr = branch.BranchNameAr,
                BranchNameEn = branch.BranchNameEn,
                AddressAr = branch.AddressAr,
                AddressEn = branch.AddressEn,
                ManagerUserId = branch.ManagerUserId,
                ManagerFullNameAr = manager?.FullNameAr,
                ManagerFullNameEn = manager?.FullNameEn,
                IsActive = branch.IsActive,
                IsDeleted = branch.IsDeleted,
                CreatedAt = branch.CreatedAt,
                GPSLat = branch.GPSLat,
                GPSLng = branch.GPSLng,
                Email = branch.Email,
                Phone = branch.Phone,
                UpdatedAt = branch.UpdatedAt
            };
        }

        private async Task<List<BranchResponseDto>> MapBranchesToDtosAsync(List<Branch> branches)
        {
            var managerUserIds = branches
                .Select(b => b.ManagerUserId)
                .Distinct()
                .ToList();

            var managers = await _context.Users
                .AsNoTracking()
                .Where(u => managerUserIds.Contains(u.Id))
                .Select(u => new
                {
                    u.Id,
                    u.FullNameAr,
                    u.FullNameEn
                })
                .ToDictionaryAsync(u => u.Id);

            return branches
                .Select(branch =>
                {
                    managers.TryGetValue(branch.ManagerUserId, out var manager);

                    return new BranchResponseDto
                    {
                        Id = branch.Id,
                        BranchNameAr = branch.BranchNameAr,
                        BranchNameEn = branch.BranchNameEn,
                        AddressAr = branch.AddressAr,
                        AddressEn = branch.AddressEn,
                        ManagerUserId = branch.ManagerUserId,
                        ManagerFullNameAr = manager?.FullNameAr,
                        ManagerFullNameEn = manager?.FullNameEn,
                        IsActive = branch.IsActive,
                        IsDeleted = branch.IsDeleted,
                        CreatedAt = branch.CreatedAt,
                        GPSLat = branch.GPSLat,
                        GPSLng = branch.GPSLng,
                        Email = branch.Email,
                        Phone = branch.Phone,
                        UpdatedAt = branch.UpdatedAt
                    };
                })
                .ToList();
        }
    }
}