using Microsoft.EntityFrameworkCore;
using QatratHayat.Application.Common.DTOS;
using QatratHayat.Application.Common.Exceptions;
using QatratHayat.Application.Features.HospitalManagement.DTOS;
using QatratHayat.Application.Features.HospitalManagement.Interfaces;
using QatratHayat.Domain.Entities;
using QatratHayat.Domain.Enums;
using QatratHayat.Infrastructure.Persistence;

namespace QatratHayat.Application.Features.HospitalManagement.Services
{
    public class HospitalManagementService : IHospitalManagementService
    {
        private readonly AppDbContext _context;

        public HospitalManagementService(AppDbContext context)
        {
            _context = context;
        }
        // ============================================================
        // Get Hospital Statistics
        // ============================================================

        public async Task<HospitalStatisticsResponseDto> GetStatisticsAsync()
        {
            var hospitalsQuery = _context.Hospitals
                .AsNoTracking()
                .Where(h => !h.IsDeleted);

            return new HospitalStatisticsResponseDto
            {
                TotalHospitals = await hospitalsQuery.CountAsync(),

                ActiveHospitals = await hospitalsQuery.CountAsync(h => h.IsActive),

                InactiveHospitals = await hospitalsQuery.CountAsync(h => !h.IsActive),

                LastUpdate = await hospitalsQuery
                    .OrderByDescending(h => h.UpdatedAt ?? h.CreatedAt)
                    .Select(h => (DateTime?)(h.UpdatedAt ?? h.CreatedAt))
                    .FirstOrDefaultAsync()
            };
        }
        // ============================================================
        // Get Available Doctors
        // ============================================================

        public async Task<List<AvailableDoctorDto>> GetAvailableDoctorsAsync(
            int? currentHospitalId = null)
        {
            var doctorRoleName = UserRole.Doctor.ToString();

            var doctorsQuery =
                from user in _context.Users.AsNoTracking()
                join userRole in _context.UserRoles.AsNoTracking()
                    on user.Id equals userRole.UserId
                join role in _context.Roles.AsNoTracking()
                    on userRole.RoleId equals role.Id
                where role.Name == doctorRoleName
                      && user.IsActive
                      && !user.IsDeleted
                      && (
                          user.HospitalId == null ||
                          (currentHospitalId.HasValue && user.HospitalId == currentHospitalId.Value)
                      )
                select user;

            var availableDoctors = await doctorsQuery
                .OrderBy(user => user.FullNameEn)
                .Select(user => new AvailableDoctorDto
                {
                    UserId = user.Id,
                    FullNameAr = user.FullNameAr,
                    FullNameEn = user.FullNameEn,
                })
                .ToListAsync();

            return availableDoctors;
        }

        // ============================================================
        // Get All Hospitals
        // ============================================================

        public async Task<PagedResultDto<HospitalResponseDto>> GetAllHospitalsAsync(
            HospitalQueryDto query)
        {
            NormalizePaging(query);

            var hospitalsQuery = _context.Hospitals
                .AsNoTracking()
                .Include(h => h.Branch)
                .Where(h => !h.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            {
                var searchTerm = query.SearchTerm.Trim();

                hospitalsQuery = hospitalsQuery.Where(h =>
                    h.HospitalNameAr.Contains(searchTerm) ||
                    h.HospitalNameEn.Contains(searchTerm) ||
                    h.AddressAR.Contains(searchTerm) ||
                    h.AddressEn.Contains(searchTerm) ||
                    h.Branch.BranchNameAr.Contains(searchTerm) ||
                    h.Branch.BranchNameEn.Contains(searchTerm));
            }

            if (query.IsActive.HasValue)
            {
                hospitalsQuery = hospitalsQuery.Where(h =>
                    h.IsActive == query.IsActive.Value);
            }

            if (query.BranchId.HasValue)
            {
                hospitalsQuery = hospitalsQuery.Where(h =>
                    h.BranchId == query.BranchId.Value);
            }

            var totalCount = await hospitalsQuery.CountAsync();

            var hospitals = await hospitalsQuery
                .OrderByDescending(h => h.CreatedAt)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            var items = hospitals
                .Select(MapHospitalToDto)
                .ToList();

            return new PagedResultDto<HospitalResponseDto>
            {
                Items = items,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                TotalCount = totalCount
            };
        }

        // ============================================================
        // Get Hospital By Id
        // ============================================================

        public async Task<HospitalResponseDto> GetHospitalByIdAsync(int hospitalId)
        {
            var hospital = await _context.Hospitals
                .AsNoTracking()
                .Include(h => h.Branch)
                .FirstOrDefaultAsync(h =>
                    h.Id == hospitalId &&
                    !h.IsDeleted);

            if (hospital is null)
            {
                throw new NotFoundException(
                    "Hospital was not found.",
                    ErrorCodes.HospitalNotFound
                );
            }

            return MapHospitalToDto(hospital);
        }

        // ============================================================
        // Add Hospital
        // ============================================================

        public async Task<HospitalResponseDto> AddHospitalAsync(AddHospitalRequestDto request)
        {
            await ValidateBranchIsActiveAsync(request.BranchId);

            await ValidateHospitalNameUniquenessAsync(
                request.HospitalNameAr,
                request.HospitalNameEn
            );

            var hospital = new Hospital
            {
                HospitalNameAr = request.HospitalNameAr.Trim(),
                HospitalNameEn = request.HospitalNameEn.Trim(),
                AddressAR = request.AddressAR.Trim(),
                AddressEn = request.AddressEn.Trim(),
                BranchId = request.BranchId,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            };

            _context.Hospitals.Add(hospital);

            await _context.SaveChangesAsync();

            var createdHospital = await _context.Hospitals
                .AsNoTracking()
                .Include(h => h.Branch)
                .FirstAsync(h => h.Id == hospital.Id);

            return MapHospitalToDto(createdHospital);
        }

        // ============================================================
        // Update Hospital
        // ============================================================

        public async Task<HospitalResponseDto> UpdateHospitalAsync(
            int hospitalId,
            UpdateHospitalRequestDto request)
        {
            var hospital = await _context.Hospitals
                .FirstOrDefaultAsync(h =>
                    h.Id == hospitalId &&
                    !h.IsDeleted);

            if (hospital is null)
            {
                throw new NotFoundException(
                    "Hospital was not found.",
                    ErrorCodes.HospitalNotFound
                );
            }

            await ValidateBranchIsActiveAsync(request.BranchId);

            await ValidateHospitalNameUniquenessAsync(
                request.HospitalNameAr,
                request.HospitalNameEn,
                excludedHospitalId: hospitalId
            );

            hospital.HospitalNameAr = request.HospitalNameAr.Trim();
            hospital.HospitalNameEn = request.HospitalNameEn.Trim();
            hospital.AddressAR = request.AddressAR.Trim();
            hospital.AddressEn = request.AddressEn.Trim();
            hospital.BranchId = request.BranchId;
            hospital.IsActive = request.IsActive;
            hospital.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var updatedHospital = await _context.Hospitals
                .AsNoTracking()
                .Include(h => h.Branch)
                .FirstAsync(h => h.Id == hospital.Id);

            return MapHospitalToDto(updatedHospital);
        }

        // ============================================================
        // Soft Delete Hospital
        // ============================================================

        public async Task SoftDeleteHospitalAsync(int hospitalId)
        {
            var hospital = await _context.Hospitals
                .FirstOrDefaultAsync(h =>
                    h.Id == hospitalId &&
                    !h.IsDeleted);

            if (hospital is null)
            {
                throw new NotFoundException(
                    "Hospital was not found.",
                    ErrorCodes.HospitalNotFound
                );
            }

            hospital.IsDeleted = true;
            hospital.IsActive = false;
            hospital.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        // ============================================================
        // Activate Hospital
        // ============================================================

        public async Task ActivateHospitalAsync(int hospitalId)
        {
            var hospital = await _context.Hospitals
                .FirstOrDefaultAsync(h =>
                    h.Id == hospitalId &&
                    !h.IsDeleted);

            if (hospital is null)
            {
                throw new NotFoundException(
                    "Hospital was not found.",
                    ErrorCodes.HospitalNotFound
                );
            }

            await ValidateBranchIsActiveAsync(hospital.BranchId);

            hospital.IsActive = true;
            hospital.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        // ============================================================
        // Deactivate Hospital
        // ============================================================

        public async Task DeactivateHospitalAsync(int hospitalId)
        {
            var hospital = await _context.Hospitals
                .FirstOrDefaultAsync(h =>
                    h.Id == hospitalId &&
                    !h.IsDeleted);

            if (hospital is null)
            {
                throw new NotFoundException(
                    "Hospital was not found.",
                    ErrorCodes.HospitalNotFound
                );
            }

            hospital.IsActive = false;
            hospital.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        // ============================================================
        // Validation Helpers
        // ============================================================

        private async Task ValidateBranchIsActiveAsync(int branchId)
        {
            var branchExists = await _context.Branches
                .AnyAsync(b =>
                    b.Id == branchId &&
                    b.IsActive &&
                    !b.IsDeleted);

            if (!branchExists)
            {
                throw new NotFoundException(
                    "Branch was not found or is inactive.",
                    ErrorCodes.BranchInactiveOrNotFound
                );
            }
        }

        private async Task ValidateHospitalNameUniquenessAsync(
            string hospitalNameAr,
            string hospitalNameEn,
            int? excludedHospitalId = null)
        {
            var normalizedNameAr = hospitalNameAr.Trim();
            var normalizedNameEn = hospitalNameEn.Trim();

            var exists = await _context.Hospitals
                .AnyAsync(h =>
                    !h.IsDeleted &&
                    (h.HospitalNameAr == normalizedNameAr ||
                     h.HospitalNameEn == normalizedNameEn) &&
                    (!excludedHospitalId.HasValue ||
                     h.Id != excludedHospitalId.Value));

            if (exists)
            {
                throw new ConflictException(
                    "A hospital with the same Arabic or English name already exists.",
                    ErrorCodes.HospitalAlreadyExists
                );
            }
        }

        private static void NormalizePaging(HospitalQueryDto query)
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
        // Mapping Helper
        // ============================================================

        private static HospitalResponseDto MapHospitalToDto(Hospital hospital)
        {
            return new HospitalResponseDto
            {
                Id = hospital.Id,
                HospitalNameAr = hospital.HospitalNameAr,
                HospitalNameEn = hospital.HospitalNameEn,
                AddressAR = hospital.AddressAR,
                AddressEn = hospital.AddressEn,
                IsActive = hospital.IsActive,
                IsDeleted = hospital.IsDeleted,
                CreatedAt = hospital.CreatedAt,
                UpdatedAt = hospital.UpdatedAt,
                BranchId = hospital.BranchId,
                BranchNameAr = hospital.Branch?.BranchNameAr,
                BranchNameEn = hospital.Branch?.BranchNameEn
            };
        }
    }
}