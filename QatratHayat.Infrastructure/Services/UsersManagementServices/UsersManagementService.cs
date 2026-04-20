using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QatratHayat.Application.Common.DTOS;
using QatratHayat.Application.Common.Exceptions;
using QatratHayat.Application.Features.UsersManagement.DTOS;
using QatratHayat.Application.Features.UsersManagement.Interfaces;
using QatratHayat.Domain.Entities;
using QatratHayat.Domain.Enums;
using QatratHayat.Infrastructure.Identity;
using QatratHayat.Infrastructure.Persistence;

namespace QatratHayat.Application.Features.UsersManagement.Services
{
    public class UsersManagementService : IUsersManagementService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        private static readonly UserRole[] StaffRoles =
        {
            UserRole.Doctor,
            UserRole.Employee,
            UserRole.BranchManager,
            UserRole.Admin,
        };

        public UsersManagementService(
            AppDbContext context,
            UserManager<ApplicationUser> userManager
        )
        {
            _context = context;
            _userManager = userManager;
        }

        // ============================================================
        // Staff Methods
        // ============================================================

        public async Task<StaffInfoResponseDto> GetStaffByIdAsync(int userId)
        {
            var user = await GetUserBaseQuery()
                .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);

            if (user is null)
            {
                throw new NotFoundException(
                    "Staff user was not found.",
                    ErrorCodes.StaffUserNotFound
                );
            }

            var roles = await GetUserRolesAsEnumsAsync(user);

            if (!roles.Any(role => StaffRoles.Contains(role)))
            {
                throw new BadRequestException(
                    "This user is not a staff member.",
                    ErrorCodes.UserIsNotStaff
                );
            }

            return MapToStaffInfoResponseDto(user, roles);
        }

        public async Task<PagedResultDto<StaffInfoResponseDto>> GetAllStaffUsersAsync(
            UserManagementQueryDto query
        )
        {
            NormalizePaging(query);

            var staffUserIds = GetStaffUserIdsQuery();

            var usersQuery = GetUserBaseQuery()
                .Where(u => !u.IsDeleted)
                .Where(u => staffUserIds.Contains(u.Id));

            usersQuery = ApplyCommonFilters(usersQuery, query);

            if (query.Role.HasValue)
            {
                if (!StaffRoles.Contains(query.Role.Value))
                {
                    throw new BadRequestException(
                        "Invalid staff role filter.",
                        ErrorCodes.InvalidStaffRole
                    );
                }

                var roleName = query.Role.Value.ToString();

                var userIdsByRole =
                    from userRole in _context.UserRoles
                    join role in _context.Roles on userRole.RoleId equals role.Id
                    where role.Name == roleName
                    select userRole.UserId;

                usersQuery = usersQuery.Where(u => userIdsByRole.Contains(u.Id));
            }

            var totalCount = await usersQuery.CountAsync();

            var users = await usersQuery
                .OrderByDescending(u => u.CreatedAt)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            var items = new List<StaffInfoResponseDto>();

            foreach (var user in users)
            {
                var roles = await GetUserRolesAsEnumsAsync(user);
                items.Add(MapToStaffInfoResponseDto(user, roles));
            }

            return new PagedResultDto<StaffInfoResponseDto>
            {
                Items = items,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                TotalCount = totalCount,
            };
        }

        public async Task<StaffInfoResponseDto> AddStaffAsync(AddStaffRequestDto dto)
        {
            ValidateStaffRole(dto.StaffRole);

            await ValidateStaffLocationAsync(dto.StaffRole, dto.BranchId, dto.HospitalId);

            var existingUserByNationalId = await GetUserBaseQuery()
                .FirstOrDefaultAsync(u => u.NationalId == dto.NationalId);

            var emailOwner = await _userManager.FindByEmailAsync(dto.Email);

            if (existingUserByNationalId is not null)
            {
                if (existingUserByNationalId.IsDeleted)
                {
                    throw new ConflictException(
                        "This national ID belongs to a deleted user account.",
                        ErrorCodes.DeletedUserCannotBePromoted
                    );
                }

                if (emailOwner is not null && emailOwner.Id != existingUserByNationalId.Id)
                {
                    throw new ConflictException(
                        "Email is already used by another user.",
                        ErrorCodes.EmailAlreadyUsed
                    );
                }

                return await PromoteExistingUserToStaffAsync(existingUserByNationalId, dto);
            }

            if (emailOwner is not null)
            {
                throw new ConflictException("Email is already used.", ErrorCodes.EmailAlreadyUsed);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,

                NationalId = dto.NationalId,

                FullNameAr = dto.FullNameAr,
                FullNameEn = dto.FullNameEn,
                DateOfBirth = dto.DateOfBirth,
                Gender = dto.Gender,

                MaritalStatus = dto.MaritalStatus,

                BranchId = GetBranchIdForRole(dto.StaffRole, dto.BranchId),
                HospitalId = GetHospitalIdForRole(dto.StaffRole, dto.HospitalId),

                IsActive = true,
                IsDeleted = false,
                IsProfileCompleted = false,

                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null,
            };

            var createResult = await _userManager.CreateAsync(user, dto.Password!);



            if (!createResult.Succeeded)
            {
                ThrowIdentityErrors(createResult, ErrorCodes.StaffCreationFailed);
            }

            var rolesToAdd = new List<string>
            {
                UserRole.Citizen.ToString(),
                dto.StaffRole.ToString(),
            };

            var addRolesResult = await _userManager.AddToRolesAsync(user, rolesToAdd);

            if (!addRolesResult.Succeeded)
            {
                ThrowIdentityErrors(addRolesResult, ErrorCodes.RoleAssignmentFailed);
            }

            var donorProfile = new DonorProfile
            {
                UserId = user.Id,

                BloodType = dto.BloodType,

                BloodTypeStatus = BloodTypeStatus.Provisional,
                EligibilityStatus = EligibilityStatus.Eligible,
                DonationCount = 0,

                iAgree = false,
                iConfirm = false,

                PermanentDeferralReason = null,
                LastDonationDate = null,
                NextEligibleDate = null,
                BloodTypeConfirmedAt = null,
                BloodTypeConfirmedByEmployeeId = null,

                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null,
            };

            _context.DonorProfiles.Add(donorProfile);

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            var createdUser = await GetUserBaseQuery().FirstAsync(u => u.Id == user.Id);

            var roles = await GetUserRolesAsEnumsAsync(createdUser);

            return MapToStaffInfoResponseDto(createdUser, roles);
        }

        public async Task<StaffInfoResponseDto> UpdateStaffAsync(
            int userId,
            UpdateStaffRequestDto dto
        )
        {
            ValidateStaffRole(dto.StaffRole);

            await ValidateStaffLocationAsync(dto.StaffRole, dto.BranchId, dto.HospitalId);

            var user = await GetUserBaseQuery()
                .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);

            if (user is null)
            {
                throw new NotFoundException(
                    "Staff user was not found.",
                    ErrorCodes.StaffUserNotFound
                );
            }

            var currentRoles = await GetUserRolesAsEnumsAsync(user);

            if (!currentRoles.Any(role => StaffRoles.Contains(role)))
            {
                throw new BadRequestException(
                    "This user is not a staff member.",
                    ErrorCodes.UserIsNotStaff
                );
            }

            var emailOwner = await _userManager.FindByEmailAsync(dto.Email);

            if (emailOwner is not null && emailOwner.Id != user.Id)
            {
                throw new ConflictException(
                    "Email is already used by another user.",
                    ErrorCodes.EmailAlreadyUsed
                );
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            user.Email = dto.Email;
            user.UserName = dto.Email;
            user.PhoneNumber = dto.PhoneNumber;
            user.IsActive = dto.IsActive;

            user.BranchId = GetBranchIdForRole(dto.StaffRole, dto.BranchId);
            user.HospitalId = GetHospitalIdForRole(dto.StaffRole, dto.HospitalId);

            user.UpdatedAt = DateTime.UtcNow;

            var updateResult = await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                ThrowIdentityErrors(updateResult, ErrorCodes.StaffUpdateFailed);
            }

            await UpdateStaffRolesAsync(user, dto.StaffRole);

            await transaction.CommitAsync();

            var updatedUser = await GetUserBaseQuery().FirstAsync(u => u.Id == user.Id);

            var updatedRoles = await GetUserRolesAsEnumsAsync(updatedUser);

            return MapToStaffInfoResponseDto(updatedUser, updatedRoles);
        }

        // ============================================================
        // Citizen Methods
        // ============================================================

        public async Task<CitizenInfoResponseDto> GetCitizenByIdAsync(int userId)
        {
            var user = await GetUserBaseQuery()
                .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);

            if (user is null)
            {
                throw new NotFoundException(
                    "Citizen user was not found.",
                    ErrorCodes.CitizenUserNotFound
                );
            }

            var roles = await GetUserRolesAsEnumsAsync(user);

            if (!roles.Contains(UserRole.Citizen))
            {
                throw new BadRequestException(
                    "This user is not a citizen.",
                    ErrorCodes.UserIsNotCitizen
                );
            }

            return MapToCitizenInfoResponseDto(user, roles);
        }

        public async Task<PagedResultDto<CitizenInfoResponseDto>> GetAllCitizenUsersAsync(
            UserManagementQueryDto query
        )
        {
            NormalizePaging(query);

            var citizenUserIds =
                from userRole in _context.UserRoles
                join role in _context.Roles on userRole.RoleId equals role.Id
                where role.Name == UserRole.Citizen.ToString()
                select userRole.UserId;

            var usersQuery = GetUserBaseQuery()
                .Where(u => !u.IsDeleted)
                .Where(u => citizenUserIds.Contains(u.Id));

            usersQuery = ApplyCommonFilters(usersQuery, query);

            var totalCount = await usersQuery.CountAsync();

            var users = await usersQuery
                .OrderByDescending(u => u.CreatedAt)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            var items = new List<CitizenInfoResponseDto>();

            foreach (var user in users)
            {
                var roles = await GetUserRolesAsEnumsAsync(user);
                items.Add(MapToCitizenInfoResponseDto(user, roles));
            }

            return new PagedResultDto<CitizenInfoResponseDto>
            {
                Items = items,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                TotalCount = totalCount,
            };
        }

        public async Task<CitizenInfoResponseDto> UpdateCitizenAsync(
            int userId,
            UpdateCitizenRequestDto dto
        )
        {
            var user = await GetUserBaseQuery()
                .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);

            if (user is null)
            {
                throw new NotFoundException(
                    "Citizen user was not found.",
                    ErrorCodes.CitizenUserNotFound
                );
            }

            var roles = await GetUserRolesAsEnumsAsync(user);

            if (!roles.Contains(UserRole.Citizen))
            {
                throw new BadRequestException(
                    "This user is not a citizen.",
                    ErrorCodes.UserIsNotCitizen
                );
            }

            var emailOwner = await _userManager.FindByEmailAsync(dto.Email);

            if (emailOwner is not null && emailOwner.Id != user.Id)
            {
                throw new ConflictException(
                    "Email is already used by another user.",
                    ErrorCodes.EmailAlreadyUsed
                );
            }

            var donorProfile = await _context.DonorProfiles.FirstOrDefaultAsync(d =>
                d.UserId == user.Id
            );

            if (donorProfile is null)
            {
                throw new NotFoundException(
                    "Donor profile was not found for this user.",
                    ErrorCodes.DonorProfileNotFound
                );
            }

            if (
                dto.EligibilityStatus == EligibilityStatus.PermDeferred
                && string.IsNullOrWhiteSpace(dto.PermanentDeferralReason)
            )
            {
                throw new BadRequestException(
                    "Permanent deferral reason is required when eligibility status is permanently deferred.",
                    ErrorCodes.PermanentDeferralReasonRequired
                );
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            user.Email = dto.Email;
            user.UserName = dto.Email;
            user.PhoneNumber = dto.PhoneNumber;
            user.IsActive = dto.IsActive;
            user.UpdatedAt = DateTime.UtcNow;

            donorProfile.BloodTypeStatus = dto.BloodTypeStatus;
            donorProfile.EligibilityStatus = dto.EligibilityStatus;

            donorProfile.PermanentDeferralReason =
                dto.EligibilityStatus == EligibilityStatus.PermDeferred
                    ? dto.PermanentDeferralReason
                    : null;

            donorProfile.UpdatedAt = DateTime.UtcNow;

            var updateResult = await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                ThrowIdentityErrors(updateResult, ErrorCodes.CitizenUpdateFailed);
            }

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            var updatedUser = await GetUserBaseQuery().FirstAsync(u => u.Id == user.Id);

            var updatedRoles = await GetUserRolesAsEnumsAsync(updatedUser);

            return MapToCitizenInfoResponseDto(updatedUser, updatedRoles);
        }

        // ============================================================
        // Shared Methods
        // ============================================================

        public async Task ActivateUserAsync(int userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.Id == userId && !u.IsDeleted
            );

            if (user is null)
            {
                throw new NotFoundException("User was not found.", ErrorCodes.UserNotFound);
            }

            user.IsActive = true;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task DeactivateUserAsync(int userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.Id == userId && !u.IsDeleted
            );

            if (user is null)
            {
                throw new NotFoundException("User was not found.", ErrorCodes.UserNotFound);
            }

            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task SoftDeleteUserAsync(int userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.Id == userId && !u.IsDeleted
            );

            if (user is null)
            {
                throw new NotFoundException("User was not found.", ErrorCodes.UserNotFound);
            }

            user.IsDeleted = true;
            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        // ============================================================
        // Query Helpers
        // ============================================================

        private IQueryable<ApplicationUser> GetUserBaseQuery()
        {
            return _context
                .Users.Include(u => u.DonorProfile)
                .Include(u => u.Branch)
                .Include(u => u.Hospital)
                .AsQueryable();
        }

        private IQueryable<int> GetStaffUserIdsQuery()
        {
            var staffRoleNames = StaffRoles.Select(role => role.ToString()).ToList();

            return from userRole in _context.UserRoles
                   join role in _context.Roles on userRole.RoleId equals role.Id
                   where staffRoleNames.Contains(role.Name!)
                   select userRole.UserId;
        }

        private IQueryable<ApplicationUser> ApplyCommonFilters(
            IQueryable<ApplicationUser> query,
            UserManagementQueryDto filter
        )
        {
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var searchTerm = filter.SearchTerm.Trim();

                query = query.Where(u =>
                    u.NationalId.Contains(searchTerm)
                    || u.FullNameAr.Contains(searchTerm)
                    || u.FullNameEn.Contains(searchTerm)
                    || u.Email!.Contains(searchTerm)
                    || (u.PhoneNumber != null && u.PhoneNumber.Contains(searchTerm))
                );
            }

            if (filter.IsActive.HasValue)
            {
                query = query.Where(u => u.IsActive == filter.IsActive.Value);
            }

            if (filter.BranchId.HasValue)
            {
                query = query.Where(u => u.BranchId == filter.BranchId.Value);
            }

            if (filter.HospitalId.HasValue)
            {
                query = query.Where(u => u.HospitalId == filter.HospitalId.Value);
            }

            return query;
        }

        private static void NormalizePaging(UserManagementQueryDto query)
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

        private StaffInfoResponseDto MapToStaffInfoResponseDto(
            ApplicationUser user,
            List<UserRole> roles
        )
        {
            var donorProfile = user.DonorProfile;

            if (donorProfile is null)
            {
                throw new NotFoundException(
                    "Donor profile was not found for this user.",
                    ErrorCodes.DonorProfileNotFound
                );
            }

            return new StaffInfoResponseDto
            {
                UserId = user.Id,
                NationalId = user.NationalId,
                Email = user.Email!,
                PhoneNumber = user.PhoneNumber!,

                FullNameAr = user.FullNameAr,
                FullNameEn = user.FullNameEn,

                Roles = roles,

                Gender = user.Gender,
                DateOfBirth = user.DateOfBirth,
                BloodType = donorProfile.BloodType,

                BranchId = user.BranchId,
                HospitalId = user.HospitalId,

                IsProfileCompleted = user.IsProfileCompleted,
                IsActive = user.IsActive,
                IsDeleted = user.IsDeleted,

                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,

                HospitalNameAr = user.Hospital?.HospitalNameAr,
                HospitalNameEn = user.Hospital?.HospitalNameEn,

                BranchNameAr = user.Branch?.BranchNameAr,
                BranchNameEn = user.Branch?.BranchNameEn,
            };
        }

        private CitizenInfoResponseDto MapToCitizenInfoResponseDto(
            ApplicationUser user,
            List<UserRole> roles
        )
        {
            var donorProfile = user.DonorProfile;

            if (donorProfile is null)
            {
                throw new NotFoundException(
                    "Donor profile was not found for this user.",
                    ErrorCodes.DonorProfileNotFound
                );
            }

            return new CitizenInfoResponseDto
            {
                UserId = user.Id,
                NationalId = user.NationalId,
                Email = user.Email!,
                PhoneNumber = user.PhoneNumber!,

                FullNameAr = user.FullNameAr,
                FullNameEn = user.FullNameEn,

                Roles = roles,

                Gender = user.Gender,
                DateOfBirth = user.DateOfBirth,
                BloodType = donorProfile.BloodType,

                BranchId = user.BranchId,
                HospitalId = user.HospitalId,

                IsProfileCompleted = user.IsProfileCompleted,
                IsActive = user.IsActive,
                IsDeleted = user.IsDeleted,

                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,

                MaritalStatus = user.MaritalStatus,
                JobTitle = user.JobTitle,
                Address = user.Address,

                BloodTypeStatus = donorProfile.BloodTypeStatus,
                EligibilityStatus = donorProfile.EligibilityStatus,
                DonationCount = donorProfile.DonationCount,

                PermanentDeferralReason = donorProfile.PermanentDeferralReason,
                LastDonationDate = donorProfile.LastDonationDate,
                NextEligibleDate = donorProfile.NextEligibleDate,
                BloodTypeConfirmedAt = donorProfile.BloodTypeConfirmedAt,
            };
        }

        // ============================================================
        // Role Helpers
        // ============================================================
        private async Task<StaffInfoResponseDto> PromoteExistingUserToStaffAsync(
            ApplicationUser user,
            AddStaffRequestDto dto
        )
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            user.Email = dto.Email;
            user.UserName = dto.Email;
            user.PhoneNumber = dto.PhoneNumber;

            user.BranchId = GetBranchIdForRole(dto.StaffRole, dto.BranchId);
            user.HospitalId = GetHospitalIdForRole(dto.StaffRole, dto.HospitalId);

            user.IsActive = true;
            user.UpdatedAt = DateTime.UtcNow;

            var updateResult = await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                ThrowIdentityErrors(updateResult, ErrorCodes.StaffUpdateFailed);
            }

            var currentRoleNames = await _userManager.GetRolesAsync(user);

            if (!currentRoleNames.Contains(UserRole.Citizen.ToString()))
            {
                var addCitizenResult = await _userManager.AddToRoleAsync(
                    user,
                    UserRole.Citizen.ToString()
                );

                if (!addCitizenResult.Succeeded)
                {
                    ThrowIdentityErrors(addCitizenResult, ErrorCodes.RoleAssignmentFailed);
                }
            }

            await UpdateStaffRolesAsync(user, dto.StaffRole);

            var donorProfile = await _context.DonorProfiles.FirstOrDefaultAsync(d =>
                d.UserId == user.Id
            );

            if (donorProfile is null)
            {
                donorProfile = new DonorProfile
                {
                    UserId = user.Id,

                    BloodType = dto.BloodType,
                    BloodTypeStatus = BloodTypeStatus.Provisional,
                    EligibilityStatus = EligibilityStatus.Eligible,
                    DonationCount = 0,

                    iAgree = false,
                    iConfirm = false,

                    PermanentDeferralReason = null,
                    LastDonationDate = null,
                    NextEligibleDate = null,
                    BloodTypeConfirmedAt = null,
                    BloodTypeConfirmedByEmployeeId = null,

                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = null,
                };

                _context.DonorProfiles.Add(donorProfile);
            }

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            var updatedUser = await GetUserBaseQuery().FirstAsync(u => u.Id == user.Id);

            var roles = await GetUserRolesAsEnumsAsync(updatedUser);

            return MapToStaffInfoResponseDto(updatedUser, roles);
        }

        private async Task<List<UserRole>> GetUserRolesAsEnumsAsync(ApplicationUser user)
        {
            var roleNames = await _userManager.GetRolesAsync(user);

            return roleNames
                .Where(roleName => Enum.TryParse<UserRole>(roleName, out _))
                .Select(Enum.Parse<UserRole>)
                .ToList();
        }

        private async Task UpdateStaffRolesAsync(ApplicationUser user, UserRole newStaffRole)
        {
            var currentRoleNames = await _userManager.GetRolesAsync(user);

            var staffRoleNamesToRemove = currentRoleNames
                .Where(roleName =>
                    Enum.TryParse<UserRole>(roleName, out var parsedRole)
                    && StaffRoles.Contains(parsedRole)
                )
                .ToList();

            if (staffRoleNamesToRemove.Any())
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(
                    user,
                    staffRoleNamesToRemove
                );

                if (!removeResult.Succeeded)
                {
                    ThrowIdentityErrors(removeResult, ErrorCodes.RoleAssignmentFailed);
                }
            }

            var refreshedRoleNames = await _userManager.GetRolesAsync(user);

            if (!refreshedRoleNames.Contains(UserRole.Citizen.ToString()))
            {
                var addCitizenResult = await _userManager.AddToRoleAsync(
                    user,
                    UserRole.Citizen.ToString()
                );

                if (!addCitizenResult.Succeeded)
                {
                    ThrowIdentityErrors(addCitizenResult, ErrorCodes.RoleAssignmentFailed);
                }
            }

            var latestRoleNames = await _userManager.GetRolesAsync(user);

            if (!latestRoleNames.Contains(newStaffRole.ToString()))
            {
                var addStaffRoleResult = await _userManager.AddToRoleAsync(
                    user,
                    newStaffRole.ToString()
                );

                if (!addStaffRoleResult.Succeeded)
                {
                    ThrowIdentityErrors(addStaffRoleResult, ErrorCodes.RoleAssignmentFailed);
                }
            }
        }

        // ============================================================
        // Validation Helpers
        // ============================================================

        private static void ValidateStaffRole(UserRole staffRole)
        {
            if (!StaffRoles.Contains(staffRole))
            {
                throw new BadRequestException("Invalid staff role.", ErrorCodes.InvalidStaffRole);
            }
        }

        private async Task ValidateStaffLocationAsync(
            UserRole staffRole,
            int? branchId,
            int? hospitalId
        )
        {
            if (staffRole == UserRole.Doctor)
            {
                if (hospitalId is null)
                {
                    throw new BadRequestException(
                        "Hospital is required for doctor users.",
                        ErrorCodes.HospitalRequiredForDoctor
                    );
                }

                var hospitalExists = await _context.Hospitals.AnyAsync(h =>
                    h.Id == hospitalId.Value && h.IsActive && !h.IsDeleted
                );

                if (!hospitalExists)
                {
                    throw new NotFoundException(
                        "Hospital was not found or is inactive.",
                        ErrorCodes.HospitalNotFound
                    );
                }

                return;
            }

            if (staffRole == UserRole.Employee || staffRole == UserRole.BranchManager)
            {
                if (branchId is null)
                {
                    throw new BadRequestException(
                        "Branch is required for employee and branch manager users.",
                        ErrorCodes.BranchRequiredForStaffRole
                    );
                }

                var branchExists = await _context.Branches.AnyAsync(b =>
                    b.Id == branchId.Value && b.IsActive && !b.IsDeleted
                );

                if (!branchExists)
                {
                    throw new NotFoundException(
                        "Branch was not found or is inactive.",
                        ErrorCodes.BranchNotFound
                    );
                }

                return;
            }

            if (staffRole == UserRole.Admin)
            {
                return;
            }

            throw new BadRequestException("Invalid staff role.", ErrorCodes.InvalidStaffRole);
        }

        private static int? GetBranchIdForRole(UserRole staffRole, int? branchId)
        {
            return staffRole is UserRole.Employee or UserRole.BranchManager ? branchId : null;
        }

        private static int? GetHospitalIdForRole(UserRole staffRole, int? hospitalId)
        {
            return staffRole == UserRole.Doctor ? hospitalId : null;
        }

        // ============================================================
        // Identity Helper
        // ============================================================

        private static void ThrowIdentityErrors(IdentityResult result, string errorCode)
        {
            var errorMessage = string.Join(" | ", result.Errors.Select(error => error.Description));

            throw new BadRequestException(
                string.IsNullOrWhiteSpace(errorMessage)
                    ? "Identity operation failed."
                    : errorMessage,
                errorCode
            );
        }
    }
}
