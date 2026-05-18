using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QatratHayat.Application.Common.DTOS;
using QatratHayat.Application.Common.Exceptions;
using QatratHayat.Application.Common.Interfaces;
using QatratHayat.Application.Features.BloodRequests.DTOS;
using QatratHayat.Application.Features.BloodRequests.Interfaces;
using QatratHayat.Domain.Entities;
using QatratHayat.Domain.Enums;
using QatratHayat.Infrastructure.Identity;
using QatratHayat.Infrastructure.Persistence;

namespace QatratHayat.Application.Features.BloodRequests.Services
{
    public class BloodRequestService : IBloodRequestService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICurrentUserService _currentUserService;

        public BloodRequestService(
            AppDbContext context,
            UserManager<ApplicationUser> userManager,
            ICurrentUserService currentUserService
        )
        {
            _context = context;
            _userManager = userManager;
            _currentUserService = currentUserService;
        }

        // ============================================================
        // Citizen Methods
        // ============================================================
        public async Task<CitizenDataResponseDto> GetCurrentCitizenDataAsync()
        {
            var currentUserId = GetCurrentUserIdOrThrow();

            var citizen = await _context
                .Users.Include(u => u.DonorProfile)
                .FirstOrDefaultAsync(u => u.Id == currentUserId && u.IsActive && !u.IsDeleted);

            if (citizen is null)
            {
                throw new NotFoundException(
                    "Current user was not found.",
                    ErrorCodes.CurrentUserNotFound
                );
            }

            await ValidateUserHasRoleAsync(citizen, UserRole.Citizen);

            if (citizen.DonorProfile is null)
            {
                throw new NotFoundException(
                    "Donor profile was not found.",
                    ErrorCodes.DonorProfileNotFound
                );
            }

            return new CitizenDataResponseDto
            {
                NationalId = citizen.NationalId,
                FullNameAr = citizen.FullNameAr,
                FullNameEn = citizen.FullNameEn,
                BloodType = citizen.DonorProfile.BloodType,
                BloodTypeDisplayName = citizen.DonorProfile.BloodType.ToDisplayName(),
            };
        }

        public async Task<CitizenDataResponseDto> LookupBeneficiaryByNationalIdAsync(
            string nationalId
        )
        {
            var currentUserId = GetCurrentUserIdOrThrow();

            var requester = await _context.Users.FirstOrDefaultAsync(u =>
                u.Id == currentUserId && u.IsActive && !u.IsDeleted
            );

            if (requester is null)
            {
                throw new NotFoundException(
                    "Current user was not found.",
                    ErrorCodes.CurrentUserNotFound
                );
            }

            await ValidateUserHasRoleAsync(requester, UserRole.Citizen);

            if (string.IsNullOrWhiteSpace(nationalId))
            {
                throw new BadRequestException(
                    "National ID is required.",
                    ErrorCodes.InvalidBeneficiaryNationalId
                );
            }

            nationalId = nationalId.Trim();

            if (nationalId.Length != 10 || !nationalId.All(char.IsDigit))
            {
                throw new BadRequestException(
                    "National ID must be exactly 10 digits.",
                    ErrorCodes.InvalidBeneficiaryNationalId
                );
            }

            var registryRecord = await _context
                .NationalRegistries.AsNoTracking()
                .FirstOrDefaultAsync(r => r.NationalId == nationalId);

            if (registryRecord is null)
            {
                throw new NotFoundException(
                    "Citizen was not found in national registry.",
                    ErrorCodes.BeneficiaryNotFoundInNationalRegistry
                );
            }

            return new CitizenDataResponseDto
            {
                NationalId = registryRecord.NationalId,
                FullNameAr = registryRecord.FullNameAr,
                FullNameEn = registryRecord.FullNameEn,
                BloodType = registryRecord.BloodType,
                BloodTypeDisplayName = registryRecord.BloodType.ToDisplayName(),
            };
        }

        public async Task<BloodRequestDetailsResponseDto> CreateAsync(CreateBloodRequestDto dto)
        {
            if (dto is null)
            {
                throw new BadRequestException("Request body is required.", ErrorCodes.BadRequest);
            }

            ValidateCreateRequest(dto);

            var currentUserId = GetCurrentUserIdOrThrow();

            var requester = await _context.Users.FirstOrDefaultAsync(u =>
                u.Id == currentUserId && u.IsActive && !u.IsDeleted
            );

            if (requester is null)
            {
                throw new NotFoundException(
                    "Current user was not found.",
                    ErrorCodes.CurrentUserNotFound
                );
            }

            await ValidateUserHasRoleAsync(requester, UserRole.Citizen);

            var hospital = await _context
                .Hospitals.Include(h => h.Branch)
                .FirstOrDefaultAsync(h => h.Id == dto.HospitalId && h.IsActive && !h.IsDeleted);

            if (hospital is null)
            {
                throw new NotFoundException(
                    "Hospital was not found or is inactive.",
                    ErrorCodes.HospitalNotFound
                );
            }

            if (hospital.Branch is null || !hospital.Branch.IsActive || hospital.Branch.IsDeleted)
            {
                throw new NotFoundException(
                    "Hospital branch was not found or is inactive.",
                    ErrorCodes.BranchInactiveOrNotFound
                );
            }

            var doctor = await _context.Users.FirstOrDefaultAsync(u =>
                u.Id == dto.DoctorId && u.HospitalId == dto.HospitalId && u.IsActive && !u.IsDeleted
            );

            if (doctor is null)
            {
                throw new NotFoundException(
                    "Doctor was not found or is not assigned to this hospital.",
                    ErrorCodes.DoctorNotFoundForHospital
                );
            }

            await ValidateUserHasRoleAsync(doctor, UserRole.Doctor);

            using var transaction = await _context.Database.BeginTransactionAsync();

            var beneficiary = await GetOrCreateBeneficiaryAsync(
                requester,
                dto.RelationshipType,
                dto.BeneficiaryNationalId
            );
            var bloodTypeSnapshot = await GetBeneficiaryBloodTypeSnapshotAsync(
                beneficiary,
                requester
            );

            var now = DateTime.UtcNow;

            var request = new BloodRequest
            {
                RelationshipType = dto.RelationshipType,

                BloodType = bloodTypeSnapshot.BloodType,
                BloodTypeStatus = bloodTypeSnapshot.BloodTypeStatus,
                UnitsNeeded = null,
                UrgencyLevel = null,

                RequestStatus = RequestStatus.PendingDoctorReview,

                CreatedAt = now,
                UpdatedAt = null,

                ClinicalNotes = null,
                DoctorApprovedAt = null,

                CancellationReason = null,
                CancelledAt = null,
                CancelledByUserId = null,

                PublishedAt = null,
                PublishedByUserId = null,

                RejectionReason = null,
                RejectedAt = null,
                RejectedByUserId = null,

                BeneficiaryId = beneficiary.Id,
                HospitalId = hospital.Id,
                BranchId = hospital.BranchId,

                RequesterUserId = requester.Id,
                DoctorId = doctor.Id,
            };

            _context.BloodRequests.Add(request);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // TODO: Send notification to selected doctor.
            // Example:
            // await _notificationService.NotifyDoctorAboutNewBloodRequestAsync(doctor.Id, request.Id);

            return await GetDetailsByIdInternalAsync(request.Id);
        }

        public async Task<PagedResultDto<BloodRequestResponseDto>> GetMyRequestsAsync(
            BloodRequestQueryDto query
        )
        {
            NormalizePaging(query);

            var currentUserId = GetCurrentUserIdOrThrow();

            var requestQuery = GetBloodRequestBaseQuery()
                .Where(r => r.RequesterUserId == currentUserId);

            requestQuery = ApplyFilters(requestQuery, query);

            return await ToPagedResponseAsync(requestQuery, query);
        }

        // ============================================================
        // Doctor Methods
        // ============================================================

        public async Task<PagedResultDto<BloodRequestResponseDto>> GetDoctorRequestsAsync(
            BloodRequestQueryDto query
        )
        {
            NormalizePaging(query);

            var currentUserId = GetCurrentUserIdOrThrow();

            var doctor = await _context.Users.FirstOrDefaultAsync(u =>
                u.Id == currentUserId && u.IsActive && !u.IsDeleted
            );

            if (doctor is null)
            {
                throw new NotFoundException(
                    "Current user was not found.",
                    ErrorCodes.CurrentUserNotFound
                );
            }

            await ValidateUserHasRoleAsync(doctor, UserRole.Doctor);

            var requestQuery = GetBloodRequestBaseQuery().Where(r => r.DoctorId == currentUserId);

            requestQuery = ApplyFilters(requestQuery, query);

            return await ToPagedResponseAsync(requestQuery, query);
        }

        public async Task<BloodRequestDetailsResponseDto> DoctorReviewAsync(
            int requestId,
            DoctorReviewBloodRequestRequestDto dto
        )
        {
            if (dto is null)
            {
                throw new BadRequestException("Request body is required.", ErrorCodes.BadRequest);
            }

            var currentUserId = GetCurrentUserIdOrThrow();

            var doctor = await _context.Users.FirstOrDefaultAsync(u =>
                u.Id == currentUserId && u.IsActive && !u.IsDeleted
            );

            if (doctor is null)
            {
                throw new NotFoundException(
                    "Current user was not found.",
                    ErrorCodes.CurrentUserNotFound
                );
            }

            await ValidateUserHasRoleAsync(doctor, UserRole.Doctor);

            var request = await GetBloodRequestBaseQuery()
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request is null)
            {
                throw new NotFoundException(
                    "Blood request was not found.",
                    ErrorCodes.BloodRequestNotFound
                );
            }

            if (request.DoctorId != currentUserId)
            {
                throw new BadRequestException(
                    "This doctor is not assigned to this blood request.",
                    ErrorCodes.DoctorNotAssignedToRequest
                );
            }

            if (request.RequestStatus != RequestStatus.PendingDoctorReview)
            {
                throw new BadRequestException(
                    "Only pending doctor review requests can be reviewed.",
                    ErrorCodes.InvalidBloodRequestStatus
                );
            }

            var now = DateTime.UtcNow;

            if (dto.IsApproved)
            {
                if (request.BloodTypeStatus == BloodTypeStatus.Confirmed)
                {
                    if (request.BloodType is null)
                    {
                        throw new BadRequestException(
                            "Confirmed blood type is missing.",
                            ErrorCodes.BloodTypeRequired
                        );
                    }

                    if (dto.BloodType is not null && dto.BloodType.Value != request.BloodType.Value)
                    {
                        throw new BadRequestException(
                            "Confirmed blood type cannot be changed by doctor.",
                            ErrorCodes.InvalidBloodRequestStatus
                        );
                    }
                }
                else
                {
                    if (dto.BloodType is null)
                    {
                        throw new BadRequestException(
                            "Blood type is required when approving a request.",
                            ErrorCodes.BloodTypeRequired
                        );
                    }
                }

                if (dto.UnitsNeeded is null || dto.UnitsNeeded.Value <= 0)
                {
                    throw new BadRequestException(
                        "Units needed must be greater than zero.",
                        ErrorCodes.UnitsNeededInvalid
                    );
                }

                if (dto.UrgencyLevel is null)
                {
                    throw new BadRequestException(
                        "Urgency level is required when approving a request.",
                        ErrorCodes.UrgencyLevelRequired
                    );
                }

                if (request.BloodTypeStatus != BloodTypeStatus.Confirmed)
                {
                    request.BloodType = dto.BloodType!.Value;
                    request.BloodTypeStatus = BloodTypeStatus.Confirmed;
                }
                request.UnitsNeeded = dto.UnitsNeeded.Value;
                request.UrgencyLevel = dto.UrgencyLevel.Value;
                request.ClinicalNotes = dto.ClinicalNotes?.Trim();

                request.RequestStatus = RequestStatus.PendingBloodBank;
                request.DoctorApprovedAt = now;
                request.UpdatedAt = now;

                // Automatic inventory check after doctor approval.
                // This will move the request to:
                // - PartiallyAllocated if compatible units are found.
                // - Shortage if no compatible units are available.
                await ReserveAvailableCompatibleUnitsTemporarilyAsync(request);

                await _context.SaveChangesAsync();

                return await GetDetailsByIdInternalAsync(request.Id);
            }

            if (string.IsNullOrWhiteSpace(dto.RejectionReason))
            {
                throw new BadRequestException(
                    "Rejection reason is required when rejecting a request.",
                    ErrorCodes.RejectionReasonRequired
                );
            }

            request.RequestStatus = RequestStatus.Rejected;
            request.RejectionReason = dto.RejectionReason.Trim();
            request.RejectedAt = now;
            request.RejectedByUserId = currentUserId;
            request.UpdatedAt = now;

            await _context.SaveChangesAsync();

            return await GetDetailsByIdInternalAsync(request.Id);
        }
        public async Task<BloodRequestDetailsResponseDto> ConfirmReceivedAsync(int requestId)
        {
            var currentUserId = GetCurrentUserIdOrThrow();

            var doctor = await _context.Users.FirstOrDefaultAsync(u =>
                u.Id == currentUserId && u.IsActive && !u.IsDeleted
            );

            if (doctor is null)
            {
                throw new NotFoundException(
                    "Current user was not found.",
                    ErrorCodes.CurrentUserNotFound
                );
            }

            await ValidateUserHasRoleAsync(doctor, UserRole.Doctor);

            var request = await GetBloodRequestBaseQuery()
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request is null)
            {
                throw new NotFoundException(
                    "Blood request was not found.",
                    ErrorCodes.BloodRequestNotFound
                );
            }

            if (request.DoctorId != currentUserId)
            {
                throw new BadRequestException(
                    "This doctor is not assigned to this blood request.",
                    ErrorCodes.DoctorNotAssignedToRequest
                );
            }

            if (request.RequestStatus != RequestStatus.Processing)
            {
                throw new BadRequestException(
                    "Only processing blood requests can be confirmed as received.",
                    ErrorCodes.InvalidBloodRequestStatus
                );
            }

            var allocatedUnits = await _context.BloodUnits
                .Where(u =>
                    u.AllocatedToRequestId == request.Id
                    && u.UnitStatus == UnitStatus.Allocated
                )
                .ToListAsync();

            if (!allocatedUnits.Any())
            {
                throw new BadRequestException(
                    "No allocated blood units were found for this request.",
                    ErrorCodes.NoReservedUnitsFound
                );
            }

            var now = DateTime.UtcNow;

            foreach (var unit in allocatedUnits)
            {
                unit.UnitStatus = UnitStatus.Used;
                unit.UpdatedAt = now;
            }

            request.FulfilledByUserId = currentUserId;
            request.RequestStatus = RequestStatus.Fulfilled;
            request.FulfilledAt = now;
            request.UpdatedAt = now;

            await _context.SaveChangesAsync();

            return await GetDetailsByIdInternalAsync(request.Id);
        }

        // ============================================================
        // Employee / Branch Manager Methods
        // ============================================================

        public async Task<PagedResultDto<BloodRequestResponseDto>> GetBranchRequestsAsync(
            BloodRequestQueryDto query
        )
        {
            NormalizePaging(query);

            var currentUserId = GetCurrentUserIdOrThrow();

            var branchId = await GetOperationalBranchIdForUserAsync(currentUserId);

            var requestQuery = GetBloodRequestBaseQuery().Where(r => r.BranchId == branchId);

            requestQuery = ApplyFilters(requestQuery, query);

            return await ToPagedResponseAsync(requestQuery, query);
        }

        public async Task<BloodRequestDetailsResponseDto> EmployeeReviewAsync(
            int requestId,
            EmployeeReviewBloodRequestRequestDto dto
        )
        {
            var currentUserId = GetCurrentUserIdOrThrow();

            var branchId = await GetOperationalBranchIdForUserAsync(currentUserId);

            var request = await GetBloodRequestBaseQuery()
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request is null)
            {
                throw new NotFoundException(
                    "Blood request was not found.",
                    ErrorCodes.BloodRequestNotFound
                );
            }

            if (request.BranchId != branchId)
            {
                throw new BadRequestException(
                    "This blood request does not belong to your branch.",
                    ErrorCodes.BloodRequestBranchMismatch
                );
            }

            if (
                request.RequestStatus != RequestStatus.PendingBloodBank
                && request.RequestStatus != RequestStatus.Shortage
                && request.RequestStatus != RequestStatus.PartiallyAllocated
            )
            {
                throw new BadRequestException(
                    "Only pending blood bank, shortage, or partially allocated requests can be rechecked.",
                    ErrorCodes.InvalidBloodRequestStatus
                );
            }

            EnsureRequestIsMedicallyCompleted(request);

            await ReserveAvailableCompatibleUnitsTemporarilyAsync(request);

            await _context.SaveChangesAsync();

            return await GetDetailsByIdInternalAsync(request.Id);
        }

        public async Task<BloodRequestDetailsResponseDto> ConfirmAllocationAsync(
            int requestId,
            ConfirmBloodRequestAllocationRequestDto dto
        )
        {
            if (dto is null)
            {
                throw new BadRequestException("Request body is required.", ErrorCodes.BadRequest);
            }

            if (!dto.ConfirmReservedUnits)
            {
                throw new BadRequestException(
                    "ConfirmReservedUnits must be true.",
                    ErrorCodes.BadRequest
                );
            }

            var currentUserId = GetCurrentUserIdOrThrow();

            var branchId = await GetOperationalBranchIdForUserAsync(currentUserId);

            var request = await GetBloodRequestBaseQuery()
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request is null)
            {
                throw new NotFoundException(
                    "Blood request was not found.",
                    ErrorCodes.BloodRequestNotFound
                );
            }

            if (request.BranchId != branchId)
            {
                throw new BadRequestException(
                    "This blood request does not belong to your branch.",
                    ErrorCodes.BloodRequestBranchMismatch
                );
            }

            if (request.RequestStatus != RequestStatus.PartiallyAllocated)
            {
                throw new BadRequestException(
                    "Only partially allocated requests can be confirmed.",
                    ErrorCodes.InvalidBloodRequestStatus
                );
            }

            EnsureRequestIsMedicallyCompleted(request);

            var now = DateTime.UtcNow;

            var reservedUnits = await _context
                .BloodUnits.Where(u =>
                    u.AllocatedToRequestId == request.Id
                    && u.UnitStatus == UnitStatus.PartiallyAllocated
                )
                .ToListAsync();

            if (!reservedUnits.Any())
            {
                throw new BadRequestException(
                    "No reserved units were found for this request.",
                    ErrorCodes.NoReservedUnitsFound
                );
            }

            foreach (var unit in reservedUnits)
            {
                unit.UnitStatus = UnitStatus.Allocated;
                unit.AllocatedAt = now;
                unit.UpdatedAt = now;
            }

            request.RequestStatus = RequestStatus.Processing;
            request.UpdatedAt = now;

            await _context.SaveChangesAsync();

            return await GetDetailsByIdInternalAsync(request.Id);
        }

        public async Task<BloodRequestDetailsResponseDto> PublishAsync(int requestId)
        {
            var currentUserId = GetCurrentUserIdOrThrow();

            var branchId = await GetOperationalBranchIdForUserAsync(currentUserId);

            var request = await GetBloodRequestBaseQuery()
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request is null)
            {
                throw new NotFoundException(
                    "Blood request was not found.",
                    ErrorCodes.BloodRequestNotFound
                );
            }

            if (request.BranchId != branchId)
            {
                throw new BadRequestException(
                    "This blood request does not belong to your branch.",
                    ErrorCodes.BloodRequestBranchMismatch
                );
            }

            if (
                request.RequestStatus != RequestStatus.Shortage
                && request.RequestStatus != RequestStatus.PartiallyAllocated
            )
            {
                throw new BadRequestException(
                    "Only shortage or partially allocated requests can be published.",
                    ErrorCodes.InvalidBloodRequestStatus
                );
            }
            var unitsRemaining = await CalculateUnitsRemainingAsync(request);

            if (unitsRemaining <= 0)
            {
                throw new BadRequestException(
                    "This blood request cannot be published because there are no remaining units needed.",
                    ErrorCodes.InvalidBloodRequestStatus
                );
            }

            var now = DateTime.UtcNow;

            request.RequestStatus = RequestStatus.Shortage;
            request.PublishedAt = now;
            request.PublishedByUserId = currentUserId;
            request.UpdatedAt = now;

            await _context.SaveChangesAsync();

            // TODO: Notify matching donors after publishing.
            // Example:
            // await _notificationService.NotifyMatchingDonorsAboutPublishedRequestAsync(request.Id);

            return await GetDetailsByIdInternalAsync(request.Id);
        }

        public async Task<BloodRequestDetailsResponseDto> RejectAsync(
            int requestId,
            RejectBloodRequestRequestDto dto
        )
        {
            if (dto is null)
            {
                throw new BadRequestException("Request body is required.", ErrorCodes.BadRequest);
            }

            if (string.IsNullOrWhiteSpace(dto.RejectionReason))
            {
                throw new BadRequestException(
                    "Rejection reason is required.",
                    ErrorCodes.RejectionReasonRequired
                );
            }

            var currentUserId = GetCurrentUserIdOrThrow();

            var branchId = await GetOperationalBranchIdForUserAsync(currentUserId);

            var request = await GetBloodRequestBaseQuery()
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request is null)
            {
                throw new NotFoundException(
                    "Blood request was not found.",
                    ErrorCodes.BloodRequestNotFound
                );
            }

            if (request.BranchId != branchId)
            {
                throw new BadRequestException(
                    "This blood request does not belong to your branch.",
                    ErrorCodes.BloodRequestBranchMismatch
                );
            }

            var canReject =
                request.RequestStatus == RequestStatus.PendingBloodBank
                || request.RequestStatus == RequestStatus.Shortage
                || request.RequestStatus == RequestStatus.PartiallyAllocated;

            if (!canReject)
            {
                throw new BadRequestException(
                    "Only pending blood bank, shortage, or partially allocated requests can be rejected.",
                    ErrorCodes.InvalidBloodRequestStatus
                );
            }

            await ReleaseTemporaryReservedUnitsAsync(request.Id);

            var now = DateTime.UtcNow;

            request.RequestStatus = RequestStatus.Rejected;
            request.RejectionReason = dto.RejectionReason.Trim();
            request.RejectedAt = now;
            request.RejectedByUserId = currentUserId;
            request.UpdatedAt = now;

            await _context.SaveChangesAsync();

            return await GetDetailsByIdInternalAsync(request.Id);
        }

        // ============================================================
        // Shared Methods
        // ============================================================

        public async Task<BloodRequestDetailsResponseDto> GetByIdAsync(int requestId)
        {
            var currentUserId = GetCurrentUserIdOrThrow();

            var request = await GetBloodRequestBaseQuery()
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request is null)
            {
                throw new NotFoundException(
                    "Blood request was not found.",
                    ErrorCodes.BloodRequestNotFound
                );
            }

            await EnsureCanAccessRequestAsync(currentUserId, request);

            return await MapToDetailsResponseDtoAsync(request);
        }

        public async Task<BloodRequestDetailsResponseDto> CancelAsync(
            int requestId,
            CancelBloodRequestDto dto
        )
        {
            if (dto is null)
            {
                throw new BadRequestException("Request body is required.", ErrorCodes.BadRequest);
            }

            if (string.IsNullOrWhiteSpace(dto.CancellationReason))
            {
                throw new BadRequestException(
                    "Cancellation reason is required.",
                    ErrorCodes.CancellationReasonRequired
                );
            }

            var currentUserId = GetCurrentUserIdOrThrow();

            var request = await GetBloodRequestBaseQuery()
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request is null)
            {
                throw new NotFoundException(
                    "Blood request was not found.",
                    ErrorCodes.BloodRequestNotFound
                );
            }

            await EnsureCanAccessRequestAsync(currentUserId, request);

            if (
                request.RequestStatus == RequestStatus.Fulfilled
                || request.RequestStatus == RequestStatus.Cancelled
                || request.RequestStatus == RequestStatus.Rejected
            )
            {
                throw new BadRequestException(
                    "This blood request cannot be cancelled in its current status.",
                    ErrorCodes.InvalidBloodRequestStatus
                );
            }

            var currentUser = await _context.Users.FirstOrDefaultAsync(u =>
                u.Id == currentUserId && u.IsActive && !u.IsDeleted
            );

            if (currentUser is null)
            {
                throw new NotFoundException(
                    "Current user was not found.",
                    ErrorCodes.CurrentUserNotFound
                );
            }

            var roles = await _userManager.GetRolesAsync(currentUser);

            if (roles.Contains(UserRole.Doctor.ToString()) && request.DoctorId == currentUserId)
            {
                var canDoctorCancel =
                    request.RequestStatus == RequestStatus.PendingDoctorReview
                    || request.RequestStatus == RequestStatus.PendingBloodBank;

                if (!canDoctorCancel)
                {
                    throw new BadRequestException(
                        "Doctor can cancel only pending doctor review or pending blood bank requests.",
                        ErrorCodes.InvalidBloodRequestStatus
                    );
                }

                if (request.RequestStatus == RequestStatus.PendingBloodBank)
                {
                    var hasBlockingBloodUnitAction = await _context.BloodUnits.AnyAsync(u =>
                        u.AllocatedToRequestId == request.Id
                        && (
                            u.UnitStatus == UnitStatus.PartiallyAllocated
                            || u.UnitStatus == UnitStatus.Allocated
                            || u.UnitStatus == UnitStatus.Used
                        )
                    );

                    if (hasBlockingBloodUnitAction)
                    {
                        throw new BadRequestException(
                            "Doctor cannot cancel this request because blood units have already been reserved, allocated, or used.",
                            ErrorCodes.InvalidBloodRequestStatus
                        );
                    }
                }
            }

            await ReleaseTemporaryReservedUnitsAsync(request.Id);

            var now = DateTime.UtcNow;

            request.RequestStatus = RequestStatus.Cancelled;
            request.CancellationReason = dto.CancellationReason.Trim();
            request.CancelledAt = now;
            request.CancelledByUserId = currentUserId;
            request.UpdatedAt = now;

            await _context.SaveChangesAsync();

            return await GetDetailsByIdInternalAsync(request.Id);
        }

        // ============================================================
        // Beneficiary Helpers
        // ============================================================

        private async Task<Beneficiary> GetOrCreateBeneficiaryAsync(
            ApplicationUser requester,
            RelationshipType relationshipType,
            string? beneficiaryNationalId
        )
        {
            if (relationshipType == RelationshipType.Self)
            {
                var existingSelfBeneficiary = await _context.Beneficiaries.FirstOrDefaultAsync(b =>
                    b.UserId == requester.Id || b.NationalId == requester.NationalId
                );

                if (existingSelfBeneficiary is not null)
                {
                    existingSelfBeneficiary.UserId = requester.Id;
                    existingSelfBeneficiary.IsTemporary = false;
                    existingSelfBeneficiary.MergedIntoUserId = requester.Id;
                    existingSelfBeneficiary.MergedAt ??= DateTime.UtcNow;

                    return existingSelfBeneficiary;
                }

                var selfBeneficiary = new Beneficiary
                {
                    NationalId = requester.NationalId,
                    FullNameAr = requester.FullNameAr,
                    FullNameEn = requester.FullNameEn,
                    IsTemporary = false,
                    CreatedAt = DateTime.UtcNow,
                    UserId = requester.Id,
                    MergedIntoUserId = requester.Id,
                    MergedAt = DateTime.UtcNow,
                };

                _context.Beneficiaries.Add(selfBeneficiary);

                await _context.SaveChangesAsync();

                return selfBeneficiary;
            }

            if (string.IsNullOrWhiteSpace(beneficiaryNationalId))
            {
                throw new BadRequestException(
                    "Beneficiary national ID is required when request is not for self.",
                    ErrorCodes.BeneficiaryRequired
                );
            }

            var nationalId = beneficiaryNationalId.Trim();

            if (nationalId.Length != 10 || !nationalId.All(char.IsDigit))
            {
                throw new BadRequestException(
                    "Beneficiary national ID must be exactly 10 digits.",
                    ErrorCodes.InvalidBeneficiaryNationalId
                );
            }

            var existingBeneficiary = await _context.Beneficiaries.FirstOrDefaultAsync(b =>
                b.NationalId == nationalId
            );

            if (existingBeneficiary is not null)
            {
                return existingBeneficiary;
            }

            var registryRecord = await _context
                .NationalRegistries.AsNoTracking()
                .FirstOrDefaultAsync(r => r.NationalId == nationalId);

            if (registryRecord is null)
            {
                throw new NotFoundException(
                    "Beneficiary was not found in national registry.",
                    ErrorCodes.BeneficiaryNotFoundInNationalRegistry
                );
            }

            var existingUser = await _context
                .Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.NationalId == nationalId && !u.IsDeleted);

            var beneficiary = new Beneficiary
            {
                NationalId = registryRecord.NationalId,
                FullNameAr = registryRecord.FullNameAr,
                FullNameEn = registryRecord.FullNameEn,
                IsTemporary = existingUser is null,
                CreatedAt = DateTime.UtcNow,
                UserId = existingUser?.Id,
                MergedIntoUserId = existingUser?.Id,
                MergedAt = existingUser is null ? null : DateTime.UtcNow,
            };

            _context.Beneficiaries.Add(beneficiary);

            await _context.SaveChangesAsync();

            return beneficiary;
        }

        private async Task<(
            BloodType? BloodType,
            BloodTypeStatus BloodTypeStatus
        )> GetBeneficiaryBloodTypeSnapshotAsync(Beneficiary beneficiary, ApplicationUser requester)
        {
            if (beneficiary.NationalId == requester.NationalId)
            {
                var requesterWithProfile = await _context
                    .Users.Include(u => u.DonorProfile)
                    .FirstOrDefaultAsync(u => u.Id == requester.Id && u.IsActive && !u.IsDeleted);

                if (requesterWithProfile?.DonorProfile is null)
                {
                    throw new NotFoundException(
                        "Donor profile was not found.",
                        ErrorCodes.DonorProfileNotFound
                    );
                }

                return (
                    requesterWithProfile.DonorProfile.BloodType,
                    requesterWithProfile.DonorProfile.BloodTypeStatus
                );
            }

            var beneficiaryUser = await _context
                .Users.Include(u => u.DonorProfile)
                .FirstOrDefaultAsync(u =>
                    u.NationalId == beneficiary.NationalId && u.IsActive && !u.IsDeleted
                );

            if (beneficiaryUser?.DonorProfile is not null)
            {
                return (
                    beneficiaryUser.DonorProfile.BloodType,
                    beneficiaryUser.DonorProfile.BloodTypeStatus
                );
            }

            var registryRecord = await _context
                .NationalRegistries.AsNoTracking()
                .FirstOrDefaultAsync(r => r.NationalId == beneficiary.NationalId);

            if (registryRecord is null)
            {
                throw new NotFoundException(
                    "Beneficiary was not found in national registry.",
                    ErrorCodes.BeneficiaryNotFoundInNationalRegistry
                );
            }

            return (registryRecord.BloodType, BloodTypeStatus.Provisional);
        }

        // ============================================================
        // Inventory / Reservation / Allocation Helpers
        // ============================================================
        private async Task<int> CalculateUnitsRemainingAsync(BloodRequest request)
        {
            var reservedOrAllocatedCount = await _context.BloodUnits.CountAsync(u =>
                u.AllocatedToRequestId == request.Id
                && (
                    u.UnitStatus == UnitStatus.PartiallyAllocated
                    || u.UnitStatus == UnitStatus.Allocated
                    || u.UnitStatus == UnitStatus.Used
                )
            );

            var unitsNeeded = request.UnitsNeeded ?? 0;

            return Math.Max(unitsNeeded - reservedOrAllocatedCount, 0);
        }

        private async Task ReserveAvailableCompatibleUnitsTemporarilyAsync(BloodRequest request)
        {
            EnsureRequestIsMedicallyCompleted(request);

            var now = DateTime.UtcNow;

            var reservedOrAllocatedCount = await _context.BloodUnits.CountAsync(u =>
                u.AllocatedToRequestId == request.Id
                && (
                    u.UnitStatus == UnitStatus.PartiallyAllocated
                    || u.UnitStatus == UnitStatus.Allocated
                    || u.UnitStatus == UnitStatus.Used
                )
            );

            var remainingUnits = request.UnitsNeeded!.Value - reservedOrAllocatedCount;

            if (remainingUnits <= 0)
            {
                request.RequestStatus = RequestStatus.PartiallyAllocated;
                request.UpdatedAt = now;
                return;
            }

            var compatibleBloodTypes = GetCompatibleDonorBloodTypes(request.BloodType!.Value);

            var requestedBloodType = request.BloodType.Value;

            var availableUnits = await _context
                .BloodUnits.Where(u =>
                    u.BranchId == request.BranchId
                    && compatibleBloodTypes.Contains(u.BloodType)
                    && u.UnitStatus == UnitStatus.Available
                    && u.ExpiresAt > now
                    && u.AllocatedToRequestId == null
                )
                .OrderBy(u => u.BloodType == requestedBloodType ? 0 : 1)
                .ThenBy(u => u.ExpiresAt)
                .Take(remainingUnits)
                .ToListAsync();

            if (!availableUnits.Any())
            {
                request.RequestStatus = RequestStatus.Shortage;
                request.UpdatedAt = now;

                return;
            }

            foreach (var unit in availableUnits)
            {
                unit.UnitStatus = UnitStatus.PartiallyAllocated;
                unit.AllocatedToRequestId = request.Id;
                unit.AllocatedAt = now;
                unit.UpdatedAt = now;
            }

            request.RequestStatus = RequestStatus.PartiallyAllocated;
            request.UpdatedAt = now;
        }

        private static IReadOnlyCollection<BloodType> GetCompatibleDonorBloodTypes(
            BloodType recipientBloodType
        )
        {
            return recipientBloodType switch
            {
                BloodType.APositive => new[]
                {
                    BloodType.APositive,
                    BloodType.ANegative,
                    BloodType.OPositive,
                    BloodType.ONegative,
                },

                BloodType.ANegative => new[] { BloodType.ANegative, BloodType.ONegative },

                BloodType.BPositive => new[]
                {
                    BloodType.BPositive,
                    BloodType.BNegative,
                    BloodType.OPositive,
                    BloodType.ONegative,
                },

                BloodType.BNegative => new[] { BloodType.BNegative, BloodType.ONegative },

                BloodType.ABPositive => new[]
                {
                    BloodType.ABPositive,
                    BloodType.ABNegative,
                    BloodType.APositive,
                    BloodType.ANegative,
                    BloodType.BPositive,
                    BloodType.BNegative,
                    BloodType.OPositive,
                    BloodType.ONegative,
                },

                BloodType.ABNegative => new[]
                {
                    BloodType.ABNegative,
                    BloodType.ANegative,
                    BloodType.BNegative,
                    BloodType.ONegative,
                },

                BloodType.OPositive => new[] { BloodType.OPositive, BloodType.ONegative },

                BloodType.ONegative => new[] { BloodType.ONegative },

                _ => Array.Empty<BloodType>(),
            };
        }

        private async Task ReleaseTemporaryReservedUnitsAsync(int requestId)
        {
            var reservedUnits = await _context
                .BloodUnits.Where(u =>
                    u.AllocatedToRequestId == requestId
                    && u.UnitStatus == UnitStatus.PartiallyAllocated
                )
                .ToListAsync();

            var now = DateTime.UtcNow;

            foreach (var unit in reservedUnits)
            {
                unit.UnitStatus = UnitStatus.Available;
                unit.AllocatedToRequestId = null;
                unit.AllocatedAt = null;
                unit.UpdatedAt = now;
            }
        }

        private static void EnsureRequestIsMedicallyCompleted(BloodRequest request)
        {
            if (request.BloodType is null)
            {
                throw new BadRequestException(
                    "Blood request blood type was not confirmed by doctor.",
                    ErrorCodes.BloodTypeRequired
                );
            }

            if (request.UnitsNeeded is null || request.UnitsNeeded.Value <= 0)
            {
                throw new BadRequestException(
                    "Blood request units needed was not defined by doctor.",
                    ErrorCodes.UnitsNeededInvalid
                );
            }

            if (request.UrgencyLevel is null)
            {
                throw new BadRequestException(
                    "Blood request urgency level was not defined by doctor.",
                    ErrorCodes.UrgencyLevelRequired
                );
            }
        }

        // ============================================================
        // Query Helpers
        // ============================================================

        private IQueryable<BloodRequest> GetBloodRequestBaseQuery()
        {
            return _context
                .BloodRequests.Include(r => r.Beneficiary)
                .Include(r => r.Hospital)
                .Include(r => r.Branch)
                .Include(r => r.BloodUnits)
                .AsQueryable();
        }

        private IQueryable<BloodRequest> ApplyFilters(
            IQueryable<BloodRequest> query,
            BloodRequestQueryDto filter
        )
        {
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var searchTerm = filter.SearchTerm.Trim();

                query = query.Where(r =>
                    r.Beneficiary.NationalId.Contains(searchTerm)
                    || r.Beneficiary.FullNameAr.Contains(searchTerm)
                    || r.Beneficiary.FullNameEn.Contains(searchTerm)
                    || r.Hospital.HospitalNameAr.Contains(searchTerm)
                    || r.Hospital.HospitalNameEn.Contains(searchTerm)
                    || r.Branch.BranchNameAr.Contains(searchTerm)
                    || r.Branch.BranchNameEn.Contains(searchTerm)
                );
            }

            if (filter.RequestStatus.HasValue)
            {
                query = query.Where(r => r.RequestStatus == filter.RequestStatus.Value);
            }

            if (filter.BloodType.HasValue)
            {
                query = query.Where(r => r.BloodType == filter.BloodType.Value);
            }

            if (filter.UrgencyLevel.HasValue)
            {
                query = query.Where(r => r.UrgencyLevel == filter.UrgencyLevel.Value);
            }

            if (filter.HospitalId.HasValue)
            {
                query = query.Where(r => r.HospitalId == filter.HospitalId.Value);
            }

            if (filter.BranchId.HasValue)
            {
                query = query.Where(r => r.BranchId == filter.BranchId.Value);
            }

            if (filter.FromDate.HasValue)
            {
                query = query.Where(r => r.CreatedAt >= filter.FromDate.Value);
            }

            if (filter.ToDate.HasValue)
            {
                query = query.Where(r => r.CreatedAt <= filter.ToDate.Value);
            }

            return query;
        }

        private static void NormalizePaging(BloodRequestQueryDto query)
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

        private async Task<PagedResultDto<BloodRequestResponseDto>> ToPagedResponseAsync(
            IQueryable<BloodRequest> query,
            BloodRequestQueryDto paging
        )
        {
            var totalCount = await query.CountAsync();

            var requests = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((paging.PageNumber - 1) * paging.PageSize)
                .Take(paging.PageSize)
                .ToListAsync();

            var items = new List<BloodRequestResponseDto>();

            foreach (var request in requests)
            {
                items.Add(await MapToResponseDtoAsync(request));
            }

            return new PagedResultDto<BloodRequestResponseDto>
            {
                Items = items,
                PageNumber = paging.PageNumber,
                PageSize = paging.PageSize,
                TotalCount = totalCount,
            };
        }

        // ============================================================
        // Authorization Helpers
        // ============================================================

        private int GetCurrentUserIdOrThrow()
        {
            var currentUserId = _currentUserService.UserId;

            if (currentUserId is null)
            {
                throw new NotFoundException(
                    "Current user was not found.",
                    ErrorCodes.CurrentUserNotFound
                );
            }

            return currentUserId.Value;
        }

        private async Task ValidateUserHasRoleAsync(ApplicationUser user, UserRole requiredRole)
        {
            var roles = await _userManager.GetRolesAsync(user);

            if (!roles.Contains(requiredRole.ToString()))
            {
                throw new BadRequestException(
                    $"User must have role {requiredRole}.",
                    ErrorCodes.UserRoleInvalid
                );
            }
        }

        private async Task<int> GetOperationalBranchIdForUserAsync(int userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.Id == userId && u.IsActive && !u.IsDeleted
            );

            if (user is null)
            {
                throw new NotFoundException(
                    "Current user was not found.",
                    ErrorCodes.CurrentUserNotFound
                );
            }

            var roles = await _userManager.GetRolesAsync(user);

            var isEmployee = roles.Contains(UserRole.Employee.ToString());
            var isBranchManager = roles.Contains(UserRole.BranchManager.ToString());

            if (!isEmployee && !isBranchManager)
            {
                throw new BadRequestException(
                    "Only employee or branch manager can perform this action.",
                    ErrorCodes.EmployeeCannotReviewRequest
                );
            }

            if (isEmployee)
            {
                if (user.BranchId is null)
                {
                    throw new BadRequestException(
                        "Employee is not assigned to a branch.",
                        ErrorCodes.EmployeeBranchNotAssigned
                    );
                }

                return user.BranchId.Value;
            }

            var branch = await _context.Branches.FirstOrDefaultAsync(b =>
                b.ManagerUserId == user.Id && b.IsActive && !b.IsDeleted
            );

            if (branch is null)
            {
                throw new BadRequestException(
                    "Branch manager is not assigned to an active branch.",
                    ErrorCodes.EmployeeBranchNotAssigned
                );
            }

            return branch.Id;
        }

        private async Task EnsureCanAccessRequestAsync(int userId, BloodRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.Id == userId && u.IsActive && !u.IsDeleted
            );

            if (user is null)
            {
                throw new NotFoundException(
                    "Current user was not found.",
                    ErrorCodes.CurrentUserNotFound
                );
            }

            var roles = await _userManager.GetRolesAsync(user);

            if (request.RequesterUserId == userId)
            {
                return;
            }

            if (request.DoctorId == userId && roles.Contains(UserRole.Doctor.ToString()))
            {
                return;
            }

            if (roles.Contains(UserRole.Admin.ToString()))
            {
                return;
            }

            if (
                roles.Contains(UserRole.Employee.ToString())
                && user.BranchId.HasValue
                && request.BranchId == user.BranchId.Value
            )
            {
                return;
            }

            if (roles.Contains(UserRole.BranchManager.ToString()))
            {
                var isManagerOfBranch = await _context.Branches.AnyAsync(b =>
                    b.Id == request.BranchId
                    && b.ManagerUserId == userId
                    && b.IsActive
                    && !b.IsDeleted
                );

                if (isManagerOfBranch)
                {
                    return;
                }
            }

            throw new BadRequestException(
                "User is not allowed to access this blood request.",
                ErrorCodes.Unauthorized
            );
        }

        // ============================================================
        // Validation Helpers
        // ============================================================

        private static void ValidateCreateRequest(CreateBloodRequestDto dto)
        {
            if (dto.HospitalId <= 0)
            {
                throw new BadRequestException("Hospital is required.", ErrorCodes.HospitalNotFound);
            }

            if (dto.DoctorId <= 0)
            {
                throw new BadRequestException(
                    "Doctor is required.",
                    ErrorCodes.DoctorNotFoundForHospital
                );
            }

            if (
                dto.RelationshipType != RelationshipType.Self
                && string.IsNullOrWhiteSpace(dto.BeneficiaryNationalId)
            )
            {
                throw new BadRequestException(
                    "Beneficiary national ID is required when request is not for self.",
                    ErrorCodes.BeneficiaryRequired
                );
            }
        }

        // ============================================================
        // Mapping Helpers
        // ============================================================

        private async Task<BloodRequestDetailsResponseDto> GetDetailsByIdInternalAsync(
            int requestId
        )
        {
            var request = await GetBloodRequestBaseQuery()
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request is null)
            {
                throw new NotFoundException(
                    "Blood request was not found.",
                    ErrorCodes.BloodRequestNotFound
                );
            }

            return await MapToDetailsResponseDtoAsync(request);
        }

        private async Task<BloodRequestResponseDto> MapToResponseDtoAsync(BloodRequest request)
        {
            var requester = await _context
                .Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == request.RequesterUserId);

            var doctor = await _context
                .Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == request.DoctorId);

            var unitsReserved = await _context.BloodUnits.CountAsync(u =>
                u.AllocatedToRequestId == request.Id
                && u.UnitStatus == UnitStatus.PartiallyAllocated
            );

            var unitsAllocated = await _context.BloodUnits.CountAsync(u =>
                u.AllocatedToRequestId == request.Id
                && (u.UnitStatus == UnitStatus.Allocated || u.UnitStatus == UnitStatus.Used)
            );

            var unitsNeeded = request.UnitsNeeded ?? 0;

            return new BloodRequestResponseDto
            {
                Id = request.Id,

                RelationshipType = request.RelationshipType,

                BloodType = request.BloodType,
                BloodTypeDisplayName = request.BloodType.HasValue
                    ? request.BloodType.Value.ToDisplayName()
                    : null,
                BloodTypeStatus = request.BloodTypeStatus,

                UnitsNeeded = request.UnitsNeeded,
                UnitsReserved = unitsReserved,
                UnitsAllocated = unitsAllocated,
                UnitsRemaining = Math.Max(unitsNeeded - unitsReserved - unitsAllocated, 0),

                UrgencyLevel = request.UrgencyLevel,
                RequestStatus = request.RequestStatus,

                CreatedAt = request.CreatedAt,
                DoctorApprovedAt = request.DoctorApprovedAt,
                PublishedAt = request.PublishedAt,
                UpdatedAt = request.UpdatedAt,

                BeneficiaryId = request.BeneficiaryId,
                BeneficiaryNationalId = request.Beneficiary.NationalId,
                BeneficiaryFullNameAr = request.Beneficiary.FullNameAr,
                BeneficiaryFullNameEn = request.Beneficiary.FullNameEn,

                HospitalId = request.HospitalId,
                HospitalNameAr = request.Hospital.HospitalNameAr,
                HospitalNameEn = request.Hospital.HospitalNameEn,

                BranchId = request.BranchId,
                BranchNameAr = request.Branch.BranchNameAr,
                BranchNameEn = request.Branch.BranchNameEn,

                RequesterUserId = request.RequesterUserId,
                RequesterFullNameAr = requester?.FullNameAr ?? string.Empty,
                RequesterFullNameEn = requester?.FullNameEn ?? string.Empty,

                DoctorId = request.DoctorId,
                DoctorFullNameAr = doctor?.FullNameAr ?? string.Empty,
                DoctorFullNameEn = doctor?.FullNameEn ?? string.Empty,
            };
        }

        private async Task<BloodRequestDetailsResponseDto> MapToDetailsResponseDtoAsync(
            BloodRequest request
        )
        {
            var baseDto = await MapToResponseDtoAsync(request);

            var allocatedUnits = await _context
                .BloodUnits.AsNoTracking()
                .Where(u => u.AllocatedToRequestId == request.Id)
                .OrderBy(u => u.ExpiresAt)
                .Select(u => new AllocatedBloodUnitDto
                {
                    Id = u.Id,
                    UnitCode = u.UnitCode,
                    BloodType = u.BloodType,
                    BloodTypeDisplayName = u.BloodType.ToDisplayName(),
                    UnitStatus = u.UnitStatus,
                    CollectionDate = u.CollectionDate,
                    ExpiresAt = u.ExpiresAt,
                    AllocatedAt = u.AllocatedAt,
                })
                .ToListAsync();

            return new BloodRequestDetailsResponseDto
            {
                Id = baseDto.Id,
                RelationshipType = baseDto.RelationshipType,

                BloodType = baseDto.BloodType,
                BloodTypeDisplayName = baseDto.BloodTypeDisplayName,
                BloodTypeStatus = baseDto.BloodTypeStatus,

                UnitsNeeded = baseDto.UnitsNeeded,
                UnitsReserved = baseDto.UnitsReserved,
                UnitsAllocated = baseDto.UnitsAllocated,
                UnitsRemaining = baseDto.UnitsRemaining,

                UrgencyLevel = baseDto.UrgencyLevel,
                RequestStatus = baseDto.RequestStatus,

                CreatedAt = baseDto.CreatedAt,
                DoctorApprovedAt = baseDto.DoctorApprovedAt,
                PublishedAt = baseDto.PublishedAt,
                UpdatedAt = baseDto.UpdatedAt,

                BeneficiaryId = baseDto.BeneficiaryId,
                BeneficiaryNationalId = baseDto.BeneficiaryNationalId,
                BeneficiaryFullNameAr = baseDto.BeneficiaryFullNameAr,
                BeneficiaryFullNameEn = baseDto.BeneficiaryFullNameEn,

                HospitalId = baseDto.HospitalId,
                HospitalNameAr = baseDto.HospitalNameAr,
                HospitalNameEn = baseDto.HospitalNameEn,

                BranchId = baseDto.BranchId,
                BranchNameAr = baseDto.BranchNameAr,
                BranchNameEn = baseDto.BranchNameEn,

                RequesterUserId = baseDto.RequesterUserId,
                RequesterFullNameAr = baseDto.RequesterFullNameAr,
                RequesterFullNameEn = baseDto.RequesterFullNameEn,

                DoctorId = baseDto.DoctorId,
                DoctorFullNameAr = baseDto.DoctorFullNameAr,
                DoctorFullNameEn = baseDto.DoctorFullNameEn,

                ClinicalNotes = request.ClinicalNotes,

                CancellationReason = request.CancellationReason,
                CancelledAt = request.CancelledAt,
                CancelledByUserId = request.CancelledByUserId,

                RejectionReason = request.RejectionReason,
                RejectedAt = request.RejectedAt,
                RejectedByUserId = request.RejectedByUserId,

                PublishedByUserId = request.PublishedByUserId,

                AllocatedBloodUnits = allocatedUnits,
            };
        }
    }
}
