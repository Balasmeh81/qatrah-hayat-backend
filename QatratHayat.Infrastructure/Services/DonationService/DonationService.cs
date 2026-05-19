using Microsoft.EntityFrameworkCore;
using QatratHayat.Application.Common.DTOS;
using QatratHayat.Application.Common.Exceptions;
using QatratHayat.Application.Features.Donations.DTOs;
using QatratHayat.Application.Features.Donations.Interfaces;
using QatratHayat.Domain.Entities;
using QatratHayat.Domain.Enums;
using QatratHayat.Infrastructure.Identity;
using QatratHayat.Infrastructure.Persistence;

namespace QatratHayat.Infrastructure.Services
{
    public class DonationService : IDonationService
    {
        private readonly AppDbContext _context;
        private readonly IBloodTypeCompatibilityService _bloodTypeCompatibilityService;
        private readonly IUnitCodeGenerator _unitCodeGenerator;
        private readonly IBloodUnitSmartAllocationService _bloodUnitSmartAllocationService;

        private const int DonationIntentExpiryHours = 6;
        private const int MinimumDonationAge = 18;
        private const int MaximumDonationAge = 65;
        private const int DonationIntervalDays = 90;
        private const int BloodUnitShelfLifeDays = 42;

        public DonationService(
            AppDbContext context,
            IBloodTypeCompatibilityService bloodTypeCompatibilityService,
            IUnitCodeGenerator unitCodeGenerator,
            IBloodUnitSmartAllocationService bloodUnitSmartAllocationService
        )
        {
            _context = context;
            _bloodTypeCompatibilityService = bloodTypeCompatibilityService;
            _unitCodeGenerator = unitCodeGenerator;
            _bloodUnitSmartAllocationService = bloodUnitSmartAllocationService;
        }

        // Citizen Methods
        public async Task<DonationEligibilityResponseDto> GetDonationEligibilityAsync(int userId)
        {
            var user = await GetUserWithDonorProfileAsync(userId);

            if (!user.IsProfileCompleted)
            {
                return new DonationEligibilityResponseDto
                {
                    CanDonate = false,
                    RequiresRegistrationScreening = true,
                    Reason = "Registration screening must be completed before donation.",
                    ErrorCode = ErrorCodes.RegistrationScreeningRequired,
                };
            }

            if (user.DonorProfile is null)
            {
                return new DonationEligibilityResponseDto
                {
                    CanDonate = false,
                    Reason = "Donor profile was not found.",
                    ErrorCode = ErrorCodes.DonorProfileRequired,
                };
            }

            var activeIntent = await GetActiveDonationIntentAsync(user.DonorProfile.Id);

            if (activeIntent is not null)
            {
                return new DonationEligibilityResponseDto
                {
                    CanDonate = false,
                    HasActiveIntent = true,
                    ActiveIntentId = activeIntent.Id,
                    EligibilityStatus = user.DonorProfile.EligibilityStatus,
                    Reason = "You already have an active donation intent.",
                    ErrorCode = ErrorCodes.ActiveDonationIntentAlreadyExists,
                };
            }

            var age = CalculateAge(user.DateOfBirth, DateTime.UtcNow);

            if (age < MinimumDonationAge || age > MaximumDonationAge)
            {
                return new DonationEligibilityResponseDto
                {
                    CanDonate = false,
                    EligibilityStatus = user.DonorProfile.EligibilityStatus,
                    Reason = "Donor age must be between 18 and 65.",
                    ErrorCode = ErrorCodes.DonorAgeNotAllowed,
                };
            }

            if (user.DonorProfile.EligibilityStatus == EligibilityStatus.TempDeferred)
            {
                return new DonationEligibilityResponseDto
                {
                    CanDonate = false,
                    EligibilityStatus = user.DonorProfile.EligibilityStatus,
                    NextEligibleDate = user.DonorProfile.NextEligibleDate,
                    Reason = "Donor is temporarily deferred.",
                    ErrorCode = ErrorCodes.DonorTemporarilyDeferred,
                };
            }

            if (user.DonorProfile.EligibilityStatus == EligibilityStatus.PermDeferred)
            {
                return new DonationEligibilityResponseDto
                {
                    CanDonate = false,
                    EligibilityStatus = user.DonorProfile.EligibilityStatus,
                    Reason = "Donor is permanently deferred.",
                    ErrorCode = ErrorCodes.DonorPermanentlyDeferred,
                };
            }

            var nextEligibleDate = GetNextEligibleDate(user.DonorProfile);

            if (nextEligibleDate.HasValue && nextEligibleDate.Value > DateTime.UtcNow)
            {
                return new DonationEligibilityResponseDto
                {
                    CanDonate = false,
                    EligibilityStatus = user.DonorProfile.EligibilityStatus,
                    NextEligibleDate = nextEligibleDate,
                    Reason = "Donation interval has not passed yet.",
                    ErrorCode = ErrorCodes.DonationIntervalNotPassed,
                };
            }

            return new DonationEligibilityResponseDto
            {
                CanDonate = true,
                EligibilityStatus = user.DonorProfile.EligibilityStatus,
                NextEligibleDate = nextEligibleDate,
                Reason = null,
                ErrorCode = null,
            };
        }

        public async Task<
            PagedResultDto<PublishedBloodRequestForDonationDto>
        > GetPublishedRequestsAsync(int userId, PublishedBloodRequestsForDonationQueryDto query)
        {
            var user = await GetUserWithDonorProfileAsync(userId);

            if (user.DonorProfile is null)
            {
                throw new BadRequestException(
                    "Donor profile was not found.",
                    ErrorCodes.DonorProfileRequired
                );
            }

            var pageNumber = query.PageNumber <= 0 ? 1 : query.PageNumber;
            var pageSize = query.PageSize <= 0 ? 10 : query.PageSize;
            pageSize = pageSize > 50 ? 50 : pageSize;

            var donorBloodType = user.DonorProfile.BloodType;

            var bloodRequestsQuery =
                from request in _context.BloodRequests.AsNoTracking()
                join beneficiary in _context.Beneficiaries.AsNoTracking()
                    on request.BeneficiaryId equals beneficiary.Id
                join branch in _context.Branches.AsNoTracking() on request.BranchId equals branch.Id
                join hospital in _context.Hospitals.AsNoTracking()
                    on request.HospitalId equals hospital.Id
                join requester in _context.Users.AsNoTracking()
                    on request.RequesterUserId equals requester.Id
                where
                    request.PublishedAt.HasValue
                    && request.BloodType.HasValue
                    && request.UnitsNeeded.HasValue
                    && (
                        request.RequestStatus == RequestStatus.Shortage
                        || request.RequestStatus == RequestStatus.PartiallyAllocated
                    )
                select new
                {
                    Request = request,
                    Beneficiary = beneficiary,
                    Branch = branch,
                    Hospital = hospital,
                    Requester = requester,
                    AllocatedOrUsedCount = _context.BloodUnits.Count(unit =>
                        unit.AllocatedToRequestId == request.Id
                        && (
                            unit.UnitStatus == UnitStatus.PartiallyAllocated
                            || unit.UnitStatus == UnitStatus.Allocated
                            || unit.UnitStatus == UnitStatus.Used
                        )
                    ),
                };

            if (query.UrgencyLevel.HasValue)
            {
                bloodRequestsQuery = bloodRequestsQuery.Where(x =>
                    x.Request.UrgencyLevel == query.UrgencyLevel.Value
                );
            }

            if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            {
                var searchTerm = query.SearchTerm.Trim();

                bloodRequestsQuery = bloodRequestsQuery.Where(x =>
                    x.Beneficiary.NationalId.Contains(searchTerm)
                    || x.Beneficiary.FullNameAr.Contains(searchTerm)
                    || x.Beneficiary.FullNameEn.Contains(searchTerm)
                );
            }

            var candidateRequests = await bloodRequestsQuery
                .Select(x => new
                {
                    x.Request,
                    x.Beneficiary,
                    x.Branch,
                    x.Hospital,
                    x.Requester,
                    x.AllocatedOrUsedCount,
                    UnitsRemaining = x.Request.UnitsNeeded!.Value - x.AllocatedOrUsedCount,
                })
                .Where(x => x.UnitsRemaining > 0)
                .OrderByDescending(x => x.Request.UrgencyLevel == UrgencyLevel.Emergency)
                .ThenBy(x => x.Request.PublishedAt)
                .ToListAsync();

            var compatibleRequests = candidateRequests
                .Where(x =>
                    _bloodTypeCompatibilityService.CanDonateTo(
                        donorBloodType,
                        x.Request.BloodType!.Value
                    )
                )
                .ToList();

            var totalCount = compatibleRequests.Count;

            var items = compatibleRequests
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new PublishedBloodRequestForDonationDto
                {
                    BloodRequestId = x.Request.Id,

                    PatientNationalId = x.Beneficiary.NationalId,
                    PatientFullNameAr = x.Beneficiary.FullNameAr,
                    PatientFullNameEn = x.Beneficiary.FullNameEn,

                    ContactPhoneNumber = x.Requester.PhoneNumber,

                    BloodType = x.Request.BloodType!.Value,
                    UrgencyLevel = x.Request.UrgencyLevel ?? UrgencyLevel.Normal,

                    BranchId = x.Request.BranchId,
                    BranchNameAr = x.Branch.BranchNameAr,
                    BranchNameEn = x.Branch.BranchNameEn,

                    HospitalId = x.Request.HospitalId,
                    HospitalNameAr = x.Hospital.HospitalNameAr,
                    HospitalNameEn = x.Hospital.HospitalNameEn,

                    UnitsNeeded = x.Request.UnitsNeeded!.Value,
                    UnitsAllocatedOrUsed = x.AllocatedOrUsedCount,
                    UnitsRemaining = x.UnitsRemaining,

                    PublishedAt = x.Request.PublishedAt!.Value,
                })
                .ToList();

            return new PagedResultDto<PublishedBloodRequestForDonationDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
            };
        }

        public async Task<PublishedBloodRequestForDonationDto> GetPublishedRequestByIdAsync(
            int userId,
            int bloodRequestId
        )
        {
            var user = await GetUserWithDonorProfileAsync(userId);

            if (user.DonorProfile is null)
            {
                throw new BadRequestException(
                    "Donor profile was not found.",
                    ErrorCodes.DonorProfileRequired
                );
            }

            var donorBloodType = user.DonorProfile.BloodType;

            var request = await (
                from bloodRequest in _context.BloodRequests.AsNoTracking()
                join beneficiary in _context.Beneficiaries.AsNoTracking()
                    on bloodRequest.BeneficiaryId equals beneficiary.Id
                join branch in _context.Branches.AsNoTracking()
                    on bloodRequest.BranchId equals branch.Id
                join hospital in _context.Hospitals.AsNoTracking()
                    on bloodRequest.HospitalId equals hospital.Id
                join requester in _context.Users.AsNoTracking()
                    on bloodRequest.RequesterUserId equals requester.Id
                where
                    bloodRequest.Id == bloodRequestId
                    && bloodRequest.PublishedAt.HasValue
                    && bloodRequest.BloodType.HasValue
                    && bloodRequest.UnitsNeeded.HasValue
                    && (
                        bloodRequest.RequestStatus == RequestStatus.Shortage
                        || bloodRequest.RequestStatus == RequestStatus.PartiallyAllocated
                    )
                select new
                {
                    Request = bloodRequest,
                    Beneficiary = beneficiary,
                    Branch = branch,
                    Hospital = hospital,
                    Requester = requester,
                    AllocatedOrUsedCount = _context.BloodUnits.Count(unit =>
                        unit.AllocatedToRequestId == bloodRequest.Id
                        && (
                            unit.UnitStatus == UnitStatus.PartiallyAllocated
                            || unit.UnitStatus == UnitStatus.Allocated
                            || unit.UnitStatus == UnitStatus.Used
                        )
                    ),
                }
            ).FirstOrDefaultAsync();

            if (request is null)
            {
                throw new NotFoundException(
                    "Published blood request was not found.",
                    ErrorCodes.BloodRequestNotFound
                );
            }

            var unitsRemaining = request.Request.UnitsNeeded!.Value - request.AllocatedOrUsedCount;

            if (unitsRemaining <= 0)
            {
                throw new BadRequestException(
                    "This blood request does not need more units.",
                    ErrorCodes.BloodRequestNotAvailableForDonation
                );
            }

            if (
                !_bloodTypeCompatibilityService.CanDonateTo(
                    donorBloodType,
                    request.Request.BloodType!.Value
                )
            )
            {
                throw new BadRequestException(
                    "Your blood type is not compatible with this request.",
                    ErrorCodes.BloodTypeNotCompatible
                );
            }

            return new PublishedBloodRequestForDonationDto
            {
                BloodRequestId = request.Request.Id,

                PatientNationalId = request.Beneficiary.NationalId,
                PatientFullNameAr = request.Beneficiary.FullNameAr,
                PatientFullNameEn = request.Beneficiary.FullNameEn,

                ContactPhoneNumber = request.Requester.PhoneNumber,

                BloodType = request.Request.BloodType!.Value,
                UrgencyLevel = request.Request.UrgencyLevel ?? UrgencyLevel.Normal,

                BranchId = request.Request.BranchId,
                BranchNameAr = request.Branch.BranchNameAr,
                BranchNameEn = request.Branch.BranchNameEn,

                HospitalId = request.Request.HospitalId,
                HospitalNameAr = request.Hospital.HospitalNameAr,
                HospitalNameEn = request.Hospital.HospitalNameEn,

                UnitsNeeded = request.Request.UnitsNeeded!.Value,
                UnitsAllocatedOrUsed = request.AllocatedOrUsedCount,
                UnitsRemaining = unitsRemaining,

                PublishedAt = request.Request.PublishedAt!.Value,
            };
        }

        public async Task<DonationIntentResponseDto> CreateGeneralDonationIntentAsync(
            int userId,
            CreateGeneralDonationIntentRequestDto request
        )
        {
            var user = await GetUserWithDonorProfileAsync(userId);

            EnsureUserCanStartDonation(user);

            await EnsureDonorHasNoActiveIntentAsync(user.DonorProfile!.Id);

            var branchExists = await _context.Branches.AnyAsync(x => x.Id == request.BranchId);

            if (!branchExists)
            {
                throw new NotFoundException("Branch not found.", ErrorCodes.BranchNotFound);
            }

            var screeningSession = await GetValidPreDonationScreeningSessionAsync(
                user,
                request.ScreeningSessionId
            );

            var now = DateTime.UtcNow;

            var donationIntent = new DonationIntent
            {
                DonationType = DonationType.General,
                DonationIntentStatus = DonationIntentStatus.Active,
                DonorProfileId = user.DonorProfile.Id,
                BranchId = request.BranchId,
                BloodRequestId = null,
                CampaignId = null,
                CreatedAt = now,
                ExpiresAt = now.AddHours(DonationIntentExpiryHours),
            };

            _context.DonationIntents.Add(donationIntent);
            await _context.SaveChangesAsync();

            screeningSession.DonationIntentId = donationIntent.Id;
            await _context.SaveChangesAsync();

            return await GetDonationIntentResponseAsync(donationIntent.Id, user.DonorProfile.Id);
        }

        public async Task<DonationIntentResponseDto> CreateRequestDonationIntentAsync(
            int userId,
            CreateRequestDonationIntentRequestDto request
        )
        {
            var user = await GetUserWithDonorProfileAsync(userId);

            EnsureUserCanStartDonation(user);

            await EnsureDonorHasNoActiveIntentAsync(user.DonorProfile!.Id);

            var bloodRequest = await _context
                .BloodRequests.Include(x => x.BloodUnits)
                .FirstOrDefaultAsync(x => x.Id == request.BloodRequestId);

            if (bloodRequest is null)
            {
                throw new NotFoundException(
                    "Blood request not found.",
                    ErrorCodes.BloodRequestNotFound
                );
            }

            ValidateRequestIsAvailableForDonation(bloodRequest, user.DonorProfile.BloodType);

            var screeningSession = await GetValidPreDonationScreeningSessionAsync(
                user,
                request.ScreeningSessionId
            );

            var now = DateTime.UtcNow;

            var donationIntent = new DonationIntent
            {
                DonationType = DonationType.Request,
                DonationIntentStatus = DonationIntentStatus.Active,
                DonorProfileId = user.DonorProfile.Id,
                BranchId = bloodRequest.BranchId,
                BloodRequestId = bloodRequest.Id,
                CampaignId = null,
                CreatedAt = now,
                ExpiresAt = now.AddHours(DonationIntentExpiryHours),
            };

            _context.DonationIntents.Add(donationIntent);
            await _context.SaveChangesAsync();

            screeningSession.DonationIntentId = donationIntent.Id;
            await _context.SaveChangesAsync();

            return await GetDonationIntentResponseAsync(donationIntent.Id, user.DonorProfile.Id);
        }

        public async Task<List<DonationIntentResponseDto>> GetMyDonationIntentsAsync(int userId)
        {
            var user = await GetUserWithDonorProfileAsync(userId);

            if (user.DonorProfile is null)
            {
                throw new BadRequestException(
                    "Donor profile was not found.",
                    ErrorCodes.DonorProfileRequired
                );
            }

            var intents = await _context
                .DonationIntents.AsNoTracking()
                .Include(x => x.Branch)
                .Include(x => x.ScreeningSessions)
                .Where(x => x.DonorProfileId == user.DonorProfile.Id)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new DonationIntentResponseDto
                {
                    Id = x.Id,
                    DonationType = x.DonationType,
                    DonationIntentStatus = x.DonationIntentStatus,
                    CreatedAt = x.CreatedAt,
                    ExpiresAt = x.ExpiresAt,
                    BranchId = x.BranchId,
                    BranchNameAr = x.Branch.BranchNameAr,
                    BranchNameEn = x.Branch.BranchNameEn,
                    BloodRequestId = x.BloodRequestId,
                    CampaignId = x.CampaignId,
                    HasReviewAnswers = x.ScreeningSessions.Any(s => s.HasReviewAnswers),
                    ScreeningSessionId = x
                        .ScreeningSessions.OrderByDescending(s => s.CreatedAt)
                        .Select(s => (int?)s.Id)
                        .FirstOrDefault(),
                })
                .ToListAsync();

            return intents;
        }

        public async Task<DonationIntentResponseDto> GetMyDonationIntentByIdAsync(
            int userId,
            int intentId
        )
        {
            var user = await GetUserWithDonorProfileAsync(userId);

            if (user.DonorProfile is null)
            {
                throw new BadRequestException(
                    "Donor profile was not found.",
                    ErrorCodes.DonorProfileRequired
                );
            }

            return await GetDonationIntentResponseAsync(intentId, user.DonorProfile.Id);
        }

        public async Task<DonationIntentResponseDto> CancelMyDonationIntentAsync(
            int userId,
            int intentId
        )
        {
            var user = await GetUserWithDonorProfileAsync(userId);

            if (user.DonorProfile is null)
            {
                throw new BadRequestException(
                    "Donor profile was not found.",
                    ErrorCodes.DonorProfileRequired
                );
            }

            var donationIntent = await _context.DonationIntents.FirstOrDefaultAsync(x =>
                x.Id == intentId && x.DonorProfileId == user.DonorProfile.Id
            );

            if (donationIntent is null)
            {
                throw new NotFoundException(
                    "Donation intent not found.",
                    ErrorCodes.DonationIntentNotFound
                );
            }

            if (donationIntent.DonationIntentStatus != DonationIntentStatus.Active)
            {
                throw new BadRequestException(
                    "Only active donation intents can be cancelled.",
                    ErrorCodes.DonationIntentNotActive
                );
            }

            donationIntent.DonationIntentStatus = DonationIntentStatus.Cancelled;

            await _context.SaveChangesAsync();

            return await GetDonationIntentResponseAsync(donationIntent.Id, user.DonorProfile.Id);
        }

        // Employee Methods
        public async Task<List<BranchDonationIntentListItemDto>> GetBranchDonationIntentsAsync(
            int employeeUserId,
            BranchDonationIntentQueryDto query
        )
        {
            var employee = await GetEmployeeWithBranchAsync(employeeUserId);

            var pageNumber = query.PageNumber <= 0 ? 1 : query.PageNumber;
            var pageSize = query.PageSize <= 0 ? 10 : query.PageSize;
            pageSize = pageSize > 50 ? 50 : pageSize;

            var donationIntentsQuery =
                from intent in _context.DonationIntents.AsNoTracking()
                join donorProfile in _context.DonorProfiles.AsNoTracking()
                    on intent.DonorProfileId equals donorProfile.Id
                join user in _context.Users.AsNoTracking() on donorProfile.UserId equals user.Id
                join branch in _context.Branches.AsNoTracking() on intent.BranchId equals branch.Id
                where intent.BranchId == employee.BranchId
                select new
                {
                    Intent = intent,
                    DonorProfile = donorProfile,
                    User = user,
                    Branch = branch,
                    HasReviewAnswers = _context.ScreeningSessions.Any(s =>
                        s.DonationIntentId == intent.Id && s.HasReviewAnswers
                    ),
                    HasUnreviewedRequiredAnswers = _context.ScreeningAnswers.Any(a =>
                        a.DonationIntentId == intent.Id
                        && a.Answer
                        && a.ScreeningQuestion.DecisionMode == ScreeningDecisionMode.ReviewWhenYes
                        && a.ReviewedAnswer == null
                    ),
                };

            if (query.Status.HasValue)
            {
                donationIntentsQuery = donationIntentsQuery.Where(x =>
                    x.Intent.DonationIntentStatus == query.Status.Value
                );
            }

            if (query.DonationType.HasValue)
            {
                donationIntentsQuery = donationIntentsQuery.Where(x =>
                    x.Intent.DonationType == query.DonationType.Value
                );
            }

            if (query.BloodType.HasValue)
            {
                donationIntentsQuery = donationIntentsQuery.Where(x =>
                    x.DonorProfile.BloodType == query.BloodType.Value
                );
            }

            if (query.FromDate.HasValue)
            {
                donationIntentsQuery = donationIntentsQuery.Where(x =>
                    x.Intent.CreatedAt >= query.FromDate.Value
                );
            }

            if (query.ToDate.HasValue)
            {
                donationIntentsQuery = donationIntentsQuery.Where(x =>
                    x.Intent.CreatedAt <= query.ToDate.Value
                );
            }

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.Trim();

                donationIntentsQuery = donationIntentsQuery.Where(x =>
                    x.User.NationalId.Contains(search)
                    || x.User.FullNameAr.Contains(search)
                    || x.User.FullNameEn.Contains(search)
                    || x.User.Email!.Contains(search)
                );
            }

            var result = await donationIntentsQuery
                .OrderByDescending(x => x.Intent.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new BranchDonationIntentListItemDto
                {
                    Id = x.Intent.Id,
                    DonationType = x.Intent.DonationType,
                    DonationIntentStatus = x.Intent.DonationIntentStatus,
                    CreatedAt = x.Intent.CreatedAt,
                    ExpiresAt = x.Intent.ExpiresAt,

                    DonorProfileId = x.DonorProfile.Id,
                    DonorUserId = x.User.Id,

                    NationalId = x.User.NationalId,
                    FullNameAr = x.User.FullNameAr,
                    FullNameEn = x.User.FullNameEn,

                    BloodType = x.DonorProfile.BloodType,
                    BloodTypeStatus = x.DonorProfile.BloodTypeStatus,
                    EligibilityStatus = x.DonorProfile.EligibilityStatus,

                    BranchId = x.Branch.Id,
                    BranchNameAr = x.Branch.BranchNameAr,
                    BranchNameEn = x.Branch.BranchNameEn,

                    BloodRequestId = x.Intent.BloodRequestId,
                    CampaignId = x.Intent.CampaignId,

                    HasReviewAnswers = x.HasReviewAnswers,
                    HasUnreviewedRequiredAnswers = x.HasUnreviewedRequiredAnswers,
                })
                .ToListAsync();

            return result;
        }

        public async Task<BranchDonationIntentDetailsDto> GetBranchDonationIntentDetailsAsync(
            int employeeUserId,
            int intentId
        )
        {
            var employee = await GetEmployeeWithBranchAsync(employeeUserId);

            var baseData = await (
                from intent in _context.DonationIntents.AsNoTracking()
                join donorProfile in _context.DonorProfiles.AsNoTracking()
                    on intent.DonorProfileId equals donorProfile.Id
                join user in _context.Users.AsNoTracking() on donorProfile.UserId equals user.Id
                join branch in _context.Branches.AsNoTracking() on intent.BranchId equals branch.Id
                where intent.Id == intentId && intent.BranchId == employee.BranchId
                select new
                {
                    Intent = intent,
                    DonorProfile = donorProfile,
                    User = user,
                    Branch = branch,
                }
            ).FirstOrDefaultAsync();

            if (baseData is null)
            {
                throw new NotFoundException(
                    "Donation intent was not found in your branch.",
                    ErrorCodes.DonationIntentNotFound
                );
            }

            var screeningSessions = await _context
                .ScreeningSessions.AsNoTracking()
                .Where(s => s.DonationIntentId == intentId)
                .OrderByDescending(s => s.CreatedAt)
                .Select(s => new ScreeningSessionReviewDto
                {
                    ScreeningSessionId = s.Id,
                    SessionType = s.SessionType,
                    ResultEligibilityStatus = s.ResultEligibilityStatus,
                    HasReviewAnswers = s.HasReviewAnswers,
                    CreatedAt = s.CreatedAt,
                    CompletedAt = s.CompletedAt,
                    Answers = s
                        .Answers.OrderBy(a => a.ScreeningQuestion.DisplayOrder)
                        .Select(a => new ScreeningAnswerReviewDto
                        {
                            AnswerId = a.Id,
                            ScreeningQuestionId = a.ScreeningQuestionId,
                            QuestionTextAr = a.ScreeningQuestion.TextAr,
                            QuestionTextEn = a.ScreeningQuestion.TextEn,
                            Answer = a.Answer,
                            ConditionalDateValue = a.ConditionalDateValue,
                            AdditionalText = a.AdditionalText,
                            RequiresReview =
                                a.Answer
                                && a.ScreeningQuestion.DecisionMode
                                    == ScreeningDecisionMode.ReviewWhenYes,

                            ReviewedAnswer = a.ReviewedAnswer,
                            ReviewedConditionalDateValue = a.ReviewedConditionalDateValue,
                            ReviewedAdditionalText = a.ReviewedAdditionalText,
                            EmployeeReviewNotes = a.EmployeeReviewNotes,
                            ReviewedByEmployeeId = a.ReviewedByEmployeeId,
                            ReviewedByEmployeeNameAr = null,
                            ReviewedByEmployeeNameEn = null,
                            ReviewedAt = a.ReviewedAt,
                        })
                        .ToList(),
                })
                .ToListAsync();

            return new BranchDonationIntentDetailsDto
            {
                Id = baseData.Intent.Id,
                DonationType = baseData.Intent.DonationType,
                DonationIntentStatus = baseData.Intent.DonationIntentStatus,
                CreatedAt = baseData.Intent.CreatedAt,
                ExpiresAt = baseData.Intent.ExpiresAt,

                DonorProfileId = baseData.DonorProfile.Id,
                DonorUserId = baseData.User.Id,

                NationalId = baseData.User.NationalId,
                FullNameAr = baseData.User.FullNameAr,
                FullNameEn = baseData.User.FullNameEn,
                PhoneNumber = baseData.User.PhoneNumber,

                BloodType = baseData.DonorProfile.BloodType,
                BloodTypeStatus = baseData.DonorProfile.BloodTypeStatus,
                EligibilityStatus = baseData.DonorProfile.EligibilityStatus,

                DonationCount = baseData.DonorProfile.DonationCount,
                LastDonationDate = baseData.DonorProfile.LastDonationDate,
                NextEligibleDate = baseData.DonorProfile.NextEligibleDate,

                BranchId = baseData.Branch.Id,
                BranchNameAr = baseData.Branch.BranchNameAr,
                BranchNameEn = baseData.Branch.BranchNameEn,

                BloodRequestId = baseData.Intent.BloodRequestId,
                CampaignId = baseData.Intent.CampaignId,

                HasReviewAnswers = screeningSessions.Any(s => s.HasReviewAnswers),
                HasUnreviewedRequiredAnswers = screeningSessions
                    .SelectMany(s => s.Answers)
                    .Any(a => a.RequiresReview && a.ReviewedAnswer == null),
                ScreeningSessions = screeningSessions,
            };
        }

        public async Task<BranchDonationIntentDetailsDto> ReviewBranchIntentScreeningAsync(
            int employeeUserId,
            int intentId,
            ReviewScreeningAnswersRequestDto request
        )
        {
            if (request.Answers.Count == 0)
            {
                throw new BadRequestException(
                    "At least one screening answer review is required.",
                    ErrorCodes.ScreeningReviewAnswersRequired
                );
            }

            var employee = await GetEmployeeWithBranchAsync(employeeUserId);

            var intent = await _context
                .DonationIntents.Include(i => i.ScreeningSessions)
                    .ThenInclude(s => s.Answers)
                        .ThenInclude(a => a.ScreeningQuestion)
                .FirstOrDefaultAsync(i => i.Id == intentId && i.BranchId == employee.BranchId);

            if (intent is null)
            {
                throw new NotFoundException(
                    "Donation intent was not found in your branch.",
                    ErrorCodes.DonationIntentNotFound
                );
            }

            if (intent.DonationIntentStatus != DonationIntentStatus.Active)
            {
                throw new BadRequestException(
                    "Only active donation intents can be reviewed.",
                    ErrorCodes.DonationIntentNotActive
                );
            }

            if (intent.ExpiresAt <= DateTime.UtcNow)
            {
                intent.DonationIntentStatus = DonationIntentStatus.Expired;
                await _context.SaveChangesAsync();

                throw new BadRequestException(
                    "Donation intent is expired.",
                    ErrorCodes.DonationIntentExpired
                );
            }

            var duplicateAnswerIds = request
                .Answers.GroupBy(answer => answer.AnswerId)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .ToList();

            if (duplicateAnswerIds.Count > 0)
            {
                throw new BadRequestException(
                    "Duplicate screening answer reviews are not allowed.",
                    ErrorCodes.DuplicateScreeningAnswerReview
                );
            }

            var intentAnswers = intent
                .ScreeningSessions.SelectMany(session => session.Answers)
                .ToDictionary(answer => answer.Id);

            foreach (var reviewAnswer in request.Answers)
            {
                if (!intentAnswers.TryGetValue(reviewAnswer.AnswerId, out var answer))
                {
                    throw new BadRequestException(
                        "One or more screening answers do not belong to this donation intent.",
                        ErrorCodes.ScreeningAnswerDoesNotBelongToIntent
                    );
                }

                answer.ReviewedAnswer = reviewAnswer.ReviewedAnswer;

                answer.ReviewedConditionalDateValue = reviewAnswer.ReviewedConditionalDateValue;

                answer.ReviewedAdditionalText = string.IsNullOrWhiteSpace(
                    reviewAnswer.ReviewedAdditionalText
                )
                    ? null
                    : reviewAnswer.ReviewedAdditionalText.Trim();

                answer.EmployeeReviewNotes = string.IsNullOrWhiteSpace(
                    reviewAnswer.EmployeeReviewNotes
                )
                    ? null
                    : reviewAnswer.EmployeeReviewNotes.Trim();

                answer.ReviewedByEmployeeId = employeeUserId;
                answer.ReviewedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return await GetBranchDonationIntentDetailsAsync(employeeUserId, intentId);
        }

        public async Task<FinalAssessmentResponseDto> SubmitFinalAssessmentAsync(
            int employeeUserId,
            int intentId,
            FinalAssessmentRequestDto request
        )
        {
            var employee = await GetEmployeeWithBranchAsync(employeeUserId);

            var intent = await _context
                .DonationIntents.Include(x => x.DonorProfile)
                .Include(x => x.ScreeningSessions)
                    .ThenInclude(s => s.Answers)
                        .ThenInclude(a => a.ScreeningQuestion)
                .FirstOrDefaultAsync(x => x.Id == intentId && x.BranchId == employee.BranchId);

            if (intent is null)
            {
                throw new NotFoundException(
                    "Donation intent was not found in your branch.",
                    ErrorCodes.DonationIntentNotFound
                );
            }

            if (intent.DonationIntentStatus != DonationIntentStatus.Active)
            {
                throw new BadRequestException(
                    "Only active donation intents can be processed.",
                    ErrorCodes.DonationIntentNotActive
                );
            }

            if (intent.ExpiresAt <= DateTime.UtcNow)
            {
                intent.DonationIntentStatus = DonationIntentStatus.Expired;
                await _context.SaveChangesAsync();

                throw new BadRequestException(
                    "Donation intent is expired.",
                    ErrorCodes.DonationIntentExpired
                );
            }
            var hasUnreviewedRequiredAnswers = intent
                .ScreeningSessions.SelectMany(session => session.Answers)
                .Any(answer =>
                    answer.Answer
                    && answer.ScreeningQuestion.DecisionMode == ScreeningDecisionMode.ReviewWhenYes
                    && answer.ReviewedAnswer == null
                );

            if (hasUnreviewedRequiredAnswers)
            {
                throw new BadRequestException(
                    "All screening answers that require review must be reviewed before submitting the final assessment.",
                    ErrorCodes.ScreeningReviewRequiredBeforeFinalAssessment
                );
            }

            ValidateFinalAssessmentRequest(request);

            if (request.FinalEligibilityStatus == FinalEligibilityStatus.Approved)
            {
                return await ApproveDonationIntentAsync(
                    employeeUserId: employeeUserId,
                    intent: intent,
                    request: request
                );
            }

            if (request.FinalEligibilityStatus == FinalEligibilityStatus.TempDeferred)
            {
                intent.DonorProfile.EligibilityStatus = EligibilityStatus.TempDeferred;
                intent.DonorProfile.NextEligibleDate = request.TemporaryDeferralEndDate;
                intent.DonorProfile.UpdatedAt = DateTime.UtcNow;

                await CreateEmployeeDeferralRecordAsync(
                    donorProfileId: intent.DonorProfileId,
                    screeningSessionId: GetLatestScreeningSessionId(intent),
                    decidedByUserId: employeeUserId,
                    deferralType: DeferralType.Temporary,
                    reason: request.FinalDecisionReason ?? "Temporary deferral by employee.",
                    endDate: request.TemporaryDeferralEndDate
                );

                intent.DonationIntentStatus = DonationIntentStatus.Cancelled;

                await _context.SaveChangesAsync();

                return new FinalAssessmentResponseDto
                {
                    DonationIntentId = intent.Id,
                    DonationIntentStatus = intent.DonationIntentStatus,
                    FinalEligibilityStatus = request.FinalEligibilityStatus,
                    Message = "Donation intent cancelled due to temporary deferral.",
                };
            }

            if (request.FinalEligibilityStatus == FinalEligibilityStatus.PermDeferred)
            {
                intent.DonorProfile.EligibilityStatus = EligibilityStatus.PermDeferred;
                intent.DonorProfile.PermanentDeferralReason = request.FinalDecisionReason;
                intent.DonorProfile.NextEligibleDate = null;
                intent.DonorProfile.UpdatedAt = DateTime.UtcNow;

                await CreateEmployeeDeferralRecordAsync(
                    donorProfileId: intent.DonorProfileId,
                    screeningSessionId: GetLatestScreeningSessionId(intent),
                    decidedByUserId: employeeUserId,
                    deferralType: DeferralType.Permanent,
                    reason: request.FinalDecisionReason ?? "Permanent deferral by employee.",
                    endDate: null
                );

                intent.DonationIntentStatus = DonationIntentStatus.Cancelled;

                await _context.SaveChangesAsync();

                return new FinalAssessmentResponseDto
                {
                    DonationIntentId = intent.Id,
                    DonationIntentStatus = intent.DonationIntentStatus,
                    FinalEligibilityStatus = request.FinalEligibilityStatus,
                    Message = "Donation intent cancelled due to permanent deferral.",
                };
            }

            if (request.FinalEligibilityStatus == FinalEligibilityStatus.Rejected)
            {
                intent.DonationIntentStatus = DonationIntentStatus.Cancelled;

                await _context.SaveChangesAsync();

                return new FinalAssessmentResponseDto
                {
                    DonationIntentId = intent.Id,
                    DonationIntentStatus = intent.DonationIntentStatus,
                    FinalEligibilityStatus = request.FinalEligibilityStatus,
                    Message = "Donation intent rejected and cancelled.",
                };
            }

            throw new BadRequestException(
                "Unsupported final eligibility status.",
                ErrorCodes.UnsupportedFinalEligibilityStatus
            );
        }

        //Helper
        private async Task<ApplicationUser> GetUserWithDonorProfileAsync(int userId)
        {
            var user = await _context
                .Users.Include(x => x.DonorProfile)
                .FirstOrDefaultAsync(x => x.Id == userId);

            if (user is null)
            {
                throw new NotFoundException("User not found.", ErrorCodes.UserNotFound);
            }

            if (!user.IsActive || user.IsDeleted)
            {
                throw new UnauthorizedException(
                    "This account is inactive.",
                    ErrorCodes.AuthAccountInactive
                );
            }

            return user;
        }

        private void EnsureUserCanStartDonation(ApplicationUser user)
        {
            if (!user.IsProfileCompleted)
            {
                throw new BadRequestException(
                    "Registration screening must be completed before donation.",
                    ErrorCodes.RegistrationScreeningRequired
                );
            }

            if (user.DonorProfile is null)
            {
                throw new BadRequestException(
                    "Donor profile was not found.",
                    ErrorCodes.DonorProfileRequired
                );
            }

            var age = CalculateAge(user.DateOfBirth, DateTime.UtcNow);

            if (age < MinimumDonationAge || age > MaximumDonationAge)
            {
                throw new BadRequestException(
                    "Donor age must be between 18 and 65.",
                    ErrorCodes.DonorAgeNotAllowed
                );
            }

            if (user.DonorProfile.EligibilityStatus == EligibilityStatus.TempDeferred)
            {
                throw new BadRequestException(
                    "Donor is temporarily deferred.",
                    ErrorCodes.DonorTemporarilyDeferred
                );
            }

            if (user.DonorProfile.EligibilityStatus == EligibilityStatus.PermDeferred)
            {
                throw new BadRequestException(
                    "Donor is permanently deferred.",
                    ErrorCodes.DonorPermanentlyDeferred
                );
            }

            var nextEligibleDate = GetNextEligibleDate(user.DonorProfile);

            if (nextEligibleDate.HasValue && nextEligibleDate.Value > DateTime.UtcNow)
            {
                throw new BadRequestException(
                    "Donation interval has not passed yet.",
                    ErrorCodes.DonationIntervalNotPassed
                );
            }
        }

        private async Task EnsureDonorHasNoActiveIntentAsync(int donorProfileId)
        {
            var hasActiveIntent = await _context.DonationIntents.AnyAsync(x =>
                x.DonorProfileId == donorProfileId
                && x.DonationIntentStatus == DonationIntentStatus.Active
                && x.ExpiresAt > DateTime.UtcNow
            );

            if (hasActiveIntent)
            {
                throw new ConflictException(
                    "You already have an active donation intent.",
                    ErrorCodes.ActiveDonationIntentAlreadyExists
                );
            }
        }

        private async Task<DonationIntent?> GetActiveDonationIntentAsync(int donorProfileId)
        {
            return await _context
                .DonationIntents.AsNoTracking()
                .Where(x =>
                    x.DonorProfileId == donorProfileId
                    && x.DonationIntentStatus == DonationIntentStatus.Active
                    && x.ExpiresAt > DateTime.UtcNow
                )
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();
        }

        private async Task<ScreeningSession> GetValidPreDonationScreeningSessionAsync(
            ApplicationUser user,
            int screeningSessionId
        )
        {
            var session = await _context.ScreeningSessions.FirstOrDefaultAsync(x =>
                x.Id == screeningSessionId
                && x.UserId == user.Id
                && x.DonorProfileId == user.DonorProfile!.Id
                && x.SessionType == ScreeningSessionType.PreDonation
            );

            if (session is null)
            {
                throw new NotFoundException(
                    "Screening session not found.",
                    ErrorCodes.ScreeningSessionNotFound
                );
            }

            if (session.DonationIntentId.HasValue)
            {
                throw new ConflictException(
                    "Screening session is already linked to a donation intent.",
                    ErrorCodes.ScreeningSessionAlreadyUsed
                );
            }

            if (session.ResultEligibilityStatus != EligibilityStatus.Eligible)
            {
                throw new BadRequestException(
                    "Screening session is not eligible for donation intent creation.",
                    ErrorCodes.ScreeningSessionNotEligible
                );
            }

            if (!session.CompletedAt.HasValue)
            {
                throw new BadRequestException(
                    "Screening session is not completed.",
                    ErrorCodes.ScreeningSessionNotCompleted
                );
            }

            return session;
        }

        private void ValidateRequestIsAvailableForDonation(
            BloodRequest bloodRequest,
            BloodType donorBloodType
        )
        {
            if (!bloodRequest.PublishedAt.HasValue)
            {
                throw new BadRequestException(
                    "Blood request is not published for donation.",
                    ErrorCodes.BloodRequestNotPublished
                );
            }

            if (
                bloodRequest.RequestStatus != RequestStatus.Shortage
                && bloodRequest.RequestStatus != RequestStatus.PartiallyAllocated
            )
            {
                throw new BadRequestException(
                    "Blood request is not available for donation.",
                    ErrorCodes.BloodRequestNotAvailableForDonation
                );
            }

            if (!bloodRequest.BloodType.HasValue)
            {
                throw new BadRequestException(
                    "Blood request does not have a confirmed blood type.",
                    ErrorCodes.BloodRequestBloodTypeMissing
                );
            }

            if (!bloodRequest.UnitsNeeded.HasValue || bloodRequest.UnitsNeeded.Value <= 0)
            {
                throw new BadRequestException(
                    "Blood request does not have valid units needed.",
                    ErrorCodes.BloodRequestUnitsMissing
                );
            }

            var allocatedOrUsedCount = CountAllocatedOrUsedUnits(bloodRequest);
            var unitsRemaining = bloodRequest.UnitsNeeded.Value - allocatedOrUsedCount;

            if (unitsRemaining <= 0)
            {
                throw new BadRequestException(
                    "Blood request does not need more units.",
                    ErrorCodes.BloodRequestDoesNotNeedMoreUnits
                );
            }

            if (
                !_bloodTypeCompatibilityService.CanDonateTo(
                    donorBloodType,
                    bloodRequest.BloodType.Value
                )
            )
            {
                throw new BadRequestException(
                    "Donor blood type is not compatible with this blood request.",
                    ErrorCodes.BloodTypeNotCompatible
                );
            }
        }

        private static int CountAllocatedOrUsedUnits(BloodRequest bloodRequest)
        {
            return bloodRequest.BloodUnits.Count(unit =>
                unit.UnitStatus == UnitStatus.PartiallyAllocated
                || unit.UnitStatus == UnitStatus.Allocated
                || unit.UnitStatus == UnitStatus.Used
            );
        }

        private async Task<DonationIntentResponseDto> GetDonationIntentResponseAsync(
            int intentId,
            int donorProfileId
        )
        {
            var response = await _context
                .DonationIntents.AsNoTracking()
                .Include(x => x.Branch)
                .Include(x => x.ScreeningSessions)
                .Where(x => x.Id == intentId && x.DonorProfileId == donorProfileId)
                .Select(x => new DonationIntentResponseDto
                {
                    Id = x.Id,
                    DonationType = x.DonationType,
                    DonationIntentStatus = x.DonationIntentStatus,
                    CreatedAt = x.CreatedAt,
                    ExpiresAt = x.ExpiresAt,
                    BranchId = x.BranchId,
                    BranchNameAr = x.Branch.BranchNameAr,
                    BranchNameEn = x.Branch.BranchNameEn,
                    BloodRequestId = x.BloodRequestId,
                    CampaignId = x.CampaignId,
                    HasReviewAnswers = x.ScreeningSessions.Any(s => s.HasReviewAnswers),
                    ScreeningSessionId = x
                        .ScreeningSessions.OrderByDescending(s => s.CreatedAt)
                        .Select(s => (int?)s.Id)
                        .FirstOrDefault(),
                })
                .FirstOrDefaultAsync();

            if (response is null)
            {
                throw new NotFoundException(
                    "Donation intent not found.",
                    ErrorCodes.DonationIntentNotFound
                );
            }

            return response;
        }

        private static DateTime? GetNextEligibleDate(DonorProfile donorProfile)
        {
            if (donorProfile.NextEligibleDate.HasValue)
            {
                return donorProfile.NextEligibleDate.Value;
            }

            if (donorProfile.LastDonationDate.HasValue)
            {
                return donorProfile.LastDonationDate.Value.AddDays(DonationIntervalDays);
            }

            return null;
        }

        private static int CalculateAge(DateTime dateOfBirth, DateTime now)
        {
            var age = now.Year - dateOfBirth.Year;

            if (dateOfBirth.Date > now.Date.AddYears(-age))
            {
                age--;
            }

            return age;
        }

        private async Task<ApplicationUser> GetEmployeeWithBranchAsync(int employeeUserId)
        {
            var employee = await _context
                .Users.Include(x => x.Branch)
                .FirstOrDefaultAsync(x => x.Id == employeeUserId);

            if (employee is null)
            {
                throw new NotFoundException("Employee user not found.", ErrorCodes.UserNotFound);
            }

            if (!employee.IsActive || employee.IsDeleted)
            {
                throw new UnauthorizedException(
                    "This account is inactive.",
                    ErrorCodes.AuthAccountInactive
                );
            }

            if (!employee.BranchId.HasValue)
            {
                throw new BadRequestException(
                    "Employee is not assigned to a branch.",
                    ErrorCodes.EmployeeBranchRequired
                );
            }

            return employee;
        }

        private void ValidateFinalAssessmentRequest(FinalAssessmentRequestDto request)
        {
            if (
                request.FinalEligibilityStatus == FinalEligibilityStatus.TempDeferred
                && !request.TemporaryDeferralEndDate.HasValue
            )
            {
                throw new BadRequestException(
                    "Temporary deferral end date is required.",
                    ErrorCodes.TemporaryDeferralEndDateRequired
                );
            }

            if (
                request.FinalEligibilityStatus == FinalEligibilityStatus.TempDeferred
                && request.TemporaryDeferralEndDate <= DateTime.UtcNow
            )
            {
                throw new BadRequestException(
                    "Temporary deferral end date must be in the future.",
                    ErrorCodes.TemporaryDeferralEndDateInvalid
                );
            }

            if (
                request.FinalEligibilityStatus != FinalEligibilityStatus.Approved
                && string.IsNullOrWhiteSpace(request.FinalDecisionReason)
            )
            {
                throw new BadRequestException(
                    "Final decision reason is required.",
                    ErrorCodes.FinalDecisionReasonRequired
                );
            }
        }

        private static int? GetLatestScreeningSessionId(DonationIntent intent)
        {
            return intent
                .ScreeningSessions.OrderByDescending(x => x.CreatedAt)
                .Select(x => (int?)x.Id)
                .FirstOrDefault();
        }

        private async Task CreateEmployeeDeferralRecordAsync(
            int donorProfileId,
            int? screeningSessionId,
            int decidedByUserId,
            DeferralType deferralType,
            string reason,
            DateTime? endDate
        )
        {
            var record = new DeferralRecord
            {
                DonorProfileId = donorProfileId,
                ScreeningSessionId = screeningSessionId,
                ScreeningQuestionId = null,
                DeferralType = deferralType,
                Reason = reason.Trim(),
                DecisionSource = DecisionSource.EmployeeOnSite,
                CreatedAt = DateTime.UtcNow,
                StartDate = DateTime.UtcNow,
                EndDate = deferralType == DeferralType.Temporary ? endDate : null,
                DecidedByUserId = decidedByUserId,
            };

            await _context.DeferralRecords.AddAsync(record);
        }

        private async Task<FinalAssessmentResponseDto> ApproveDonationIntentAsync(
            int employeeUserId,
            DonationIntent intent,
            FinalAssessmentRequestDto request
        )
        {
            if (
                intent.DonorProfile.BloodTypeStatus != BloodTypeStatus.Confirmed
                && !request.ConfirmBloodType
            )
            {
                throw new BadRequestException(
                    "Donor blood type must be confirmed before approving the donation.",
                    ErrorCodes.DonorBloodTypeMustBeConfirmed
                );
            }

            if (request.ConfirmBloodType)
            {
                if (!request.ConfirmedBloodType.HasValue)
                {
                    throw new BadRequestException(
                        "Confirmed blood type is required when ConfirmBloodType is true.",
                        ErrorCodes.ConfirmedBloodTypeRequired
                    );
                }

                intent.DonorProfile.BloodType = request.ConfirmedBloodType.Value;
                intent.DonorProfile.BloodTypeStatus = BloodTypeStatus.Confirmed;
                intent.DonorProfile.BloodTypeConfirmedAt = DateTime.UtcNow;
                intent.DonorProfile.BloodTypeConfirmedByEmployeeId = employeeUserId;
                intent.DonorProfile.UpdatedAt = DateTime.UtcNow;
            }

            var now = DateTime.UtcNow;

            await using var transaction = await _context.Database.BeginTransactionAsync();

            var donation = new Donation
            {
                DonationType = intent.DonationType,
                InitialEligibilityStatus = InitialEligibilityStatus.Passed,
                FinalEligibilityStatus = FinalEligibilityStatus.Approved,
                CreatedAt = now,
                FinalDecisionReason = string.IsNullOrWhiteSpace(request.FinalDecisionReason)
                    ? null
                    : request.FinalDecisionReason.Trim(),

                DonorProfileId = intent.DonorProfileId,
                EmployeeUserId = employeeUserId,
                BranchId = intent.BranchId,
                BloodRequestId = intent.BloodRequestId,
                CampaignId = intent.CampaignId,
            };

            _context.Donations.Add(donation);
            await _context.SaveChangesAsync();

            var unitCode = await _unitCodeGenerator.GenerateUniqueUnitCodeAsync();

            var bloodUnit = new BloodUnit
            {
                UnitCode = unitCode,
                BloodType = intent.DonorProfile.BloodType,
                CollectionDate = now,
                ExpiresAt = now.AddDays(BloodUnitShelfLifeDays),
                UnitStatus = UnitStatus.Available,
                CreatedAt = now,

                AllocatedAt = null,
                DisposalDate = null,
                DisposalReason = null,
                DeallocationNote = null,

                AllocatedToRequestId = null,
                BranchId = intent.BranchId,
                DonationId = donation.Id,
                DisposedByEmployeeId = null,
            };

            _context.BloodUnits.Add(bloodUnit);

            await _context.SaveChangesAsync();

            await _bloodUnitSmartAllocationService.AllocateBloodUnitAsync(bloodUnit, donation);

            intent.DonationIntentStatus = DonationIntentStatus.Completed;

            intent.DonorProfile.DonationCount += 1;
            intent.DonorProfile.LastDonationDate = now;
            intent.DonorProfile.NextEligibleDate = now.AddDays(DonationIntervalDays);
            intent.DonorProfile.EligibilityStatus = EligibilityStatus.Eligible;
            intent.DonorProfile.UpdatedAt = now;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new FinalAssessmentResponseDto
            {
                DonationIntentId = intent.Id,
                DonationIntentStatus = intent.DonationIntentStatus,
                FinalEligibilityStatus = FinalEligibilityStatus.Approved,
                DonationId = donation.Id,
                BloodUnitId = bloodUnit.Id,
                Message = "Donation approved successfully. Donation and blood unit were created.",
            };
        }
    }
}
