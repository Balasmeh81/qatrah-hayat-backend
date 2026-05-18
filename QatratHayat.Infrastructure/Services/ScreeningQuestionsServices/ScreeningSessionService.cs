using Microsoft.EntityFrameworkCore;
using QatratHayat.Application.Common.Exceptions;
using QatratHayat.Application.Features.ScreeningQuestions.DTOs;
using QatratHayat.Application.Features.ScreeningQuestions.Interfaces;
using QatratHayat.Domain.Entities;
using QatratHayat.Domain.Enums;
using QatratHayat.Infrastructure.Persistence;

namespace QatratHayat.Infrastructure.Services
{
    public class ScreeningSessionService : IScreeningSessionService
    {
        private readonly AppDbContext _context;

        public ScreeningSessionService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<SubmittedScreeningResponseDTO> SubmitScreeningQuestionsAsync(
            int userId,
            SubmittedScreeningQuestionsRequestDTO request
        )
        {
            var user = await _context.Users
                .Include(x => x.DonorProfile)
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

            if (request.SessionType == ScreeningSessionType.Registration)
            {
                return await HandleRegistrationAsync(user, request);
            }

            if (request.SessionType == ScreeningSessionType.PreDonation)
            {
                return await HandlePreDonationAsync(user, request);
            }

            throw new BadRequestException(
                "Unsupported screening session type.",
                ErrorCodes.UnsupportedScreeningSessionType
            );
        }

        private async Task<SubmittedScreeningResponseDTO> HandleRegistrationAsync(
            Identity.ApplicationUser user,
            SubmittedScreeningQuestionsRequestDTO request
        )
        {
            if (user.IsProfileCompleted)
            {
                throw new BadRequestException(
                    "Registration screening already completed.",
                    ErrorCodes.RegistrationAlreadyCompleted
                );
            }

            var activeQuestions = await GetActiveQuestionsForUserAsync(
                ScreeningSessionType.Registration,
                user.Gender
            );

            ValidateQuestionsAndAnswers(request, activeQuestions);

            var evaluation = EvaluateScreeningAnswers(request.Answers, activeQuestions);

            var session = new ScreeningSession
            {
                SessionType = ScreeningSessionType.Registration,
                UserId = user.Id,
                DonorProfileId = user.DonorProfile?.Id,
                DonationIntentId = null,
                ResultEligibilityStatus = evaluation.ResultEligibilityStatus,
                HasReviewAnswers = evaluation.HasReviewAnswers,
                CreatedAt = DateTime.UtcNow,
                CompletedAt = DateTime.UtcNow,
            };

            _context.ScreeningSessions.Add(session);
            await _context.SaveChangesAsync();

            var answers = CreateScreeningAnswers(
                user.Id,
                user.DonorProfile?.Id,
                session.Id,
                null,
                request.Answers
            );

            _context.ScreeningAnswers.AddRange(answers);

            if (user.DonorProfile is not null)
            {
                ApplyEligibilityToDonorProfile(user.DonorProfile, evaluation);
            }

            await CreateDeferralRecordsIfNeededAsync(
                donorProfileId: user.DonorProfile?.Id,
                screeningSessionId: session.Id,
                submittedAnswers: request.Answers,
                activeQuestions: activeQuestions
            );

            user.IsProfileCompleted = true;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return BuildSubmittedScreeningResponse(
                session,
                user.IsProfileCompleted,
                user.DonorProfile?.NextEligibleDate,
                answers.Count
            );
        }

        private async Task<SubmittedScreeningResponseDTO> HandlePreDonationAsync(
            Identity.ApplicationUser user,
            SubmittedScreeningQuestionsRequestDTO request
        )
        {
            if (!user.IsProfileCompleted)
            {
                throw new BadRequestException(
                    "Registration screening must be completed before pre-donation screening.",
                    ErrorCodes.RegistrationScreeningRequired
                );
            }

            if (user.DonorProfile is null)
            {
                throw new BadRequestException(
                    "Donor profile is required for PreDonation screening.",
                    ErrorCodes.DonorProfileRequired
                );
            }

            var activeQuestions = await GetActiveQuestionsForUserAsync(
                ScreeningSessionType.PreDonation,
                user.Gender
            );

            ValidateQuestionsAndAnswers(request, activeQuestions);

            var evaluation = EvaluateScreeningAnswers(request.Answers, activeQuestions);

            var session = new ScreeningSession
            {
                SessionType = ScreeningSessionType.PreDonation,
                UserId = user.Id,
                DonorProfileId = user.DonorProfile.Id,
                DonationIntentId = null,
                ResultEligibilityStatus = evaluation.ResultEligibilityStatus,
                HasReviewAnswers = evaluation.HasReviewAnswers,
                CreatedAt = DateTime.UtcNow,
                CompletedAt = DateTime.UtcNow,
            };

            _context.ScreeningSessions.Add(session);
            await _context.SaveChangesAsync();

            var answers = CreateScreeningAnswers(
                user.Id,
                user.DonorProfile.Id,
                session.Id,
                null,
                request.Answers
            );

            _context.ScreeningAnswers.AddRange(answers);

            ApplyEligibilityToDonorProfile(user.DonorProfile, evaluation);

            await CreateDeferralRecordsIfNeededAsync(
                donorProfileId: user.DonorProfile.Id,
                screeningSessionId: session.Id,
                submittedAnswers: request.Answers,
                activeQuestions: activeQuestions
            );

            await _context.SaveChangesAsync();

            return BuildSubmittedScreeningResponse(
                session,
                user.IsProfileCompleted,
                user.DonorProfile.NextEligibleDate,
                answers.Count
            );
        }

        private async Task<List<ScreeningQuestion>> GetActiveQuestionsForUserAsync(
            ScreeningSessionType sessionType,
            Gender gender
        )
        {
            var query = _context.ScreeningQuestions
                .Where(q => q.IsActive && q.SessionType == sessionType);

            if (gender != Gender.Female)
            {
                query = query.Where(q => !q.IsForFemaleOnly);
            }

            return await query
                .OrderBy(q => q.DisplayOrder)
                .ToListAsync();
        }

        private void ValidateQuestionsAndAnswers(
            SubmittedScreeningQuestionsRequestDTO request,
            List<ScreeningQuestion> activeQuestions
        )
        {
            if (!activeQuestions.Any())
            {
                throw new BadRequestException(
                    "No active screening questions found.",
                    ErrorCodes.NoActiveScreeningQuestions
                );
            }

            var activeQuestionIds = activeQuestions.Select(q => q.Id).ToHashSet();
            var submittedQuestionIds = request.Answers.Select(a => a.ScreeningQuestionId).ToList();

            if (submittedQuestionIds.Count != submittedQuestionIds.Distinct().Count())
            {
                throw new BadRequestException(
                    "Duplicate question answers are not allowed.",
                    ErrorCodes.DuplicateQuestionAnswers
                );
            }

            var invalidQuestionIds = submittedQuestionIds
                .Where(id => !activeQuestionIds.Contains(id))
                .Distinct()
                .ToList();

            if (invalidQuestionIds.Any())
            {
                throw new BadRequestException(
                    $"Invalid question ids: {string.Join(", ", invalidQuestionIds)}",
                    ErrorCodes.InvalidQuestionIds
                );
            }

            var missingQuestionIds = activeQuestionIds
                .Where(id => !submittedQuestionIds.Contains(id))
                .ToList();

            if (missingQuestionIds.Any())
            {
                throw new BadRequestException(
                    $"All active questions must be answered. Missing ids: {string.Join(", ", missingQuestionIds)}",
                    ErrorCodes.MissingQuestionAnswers
                );
            }

            foreach (var submittedAnswer in request.Answers)
            {
                var question = activeQuestions.First(q =>
                    q.Id == submittedAnswer.ScreeningQuestionId
                );

                if (
                    submittedAnswer.Answer
                    && question.RequiresDateValue
                    && !submittedAnswer.ConditionalDateValue.HasValue
                )
                {
                    throw new BadRequestException(
                        $"Question '{question.TextEn}' requires a date value.",
                        ErrorCodes.DateValueRequired
                    );
                }

                if (!submittedAnswer.Answer && submittedAnswer.ConditionalDateValue.HasValue)
                {
                    throw new BadRequestException(
                        $"Question '{question.TextEn}' should not contain a date value.",
                        ErrorCodes.DateValueNotAllowed
                    );
                }

                if (
                    submittedAnswer.Answer
                    && question.RequiresAdditionalText
                    && string.IsNullOrWhiteSpace(submittedAnswer.AdditionalText)
                )
                {
                    throw new BadRequestException(
                        $"Question '{question.TextEn}' requires additional text.",
                        ErrorCodes.AdditionalTextRequired
                    );
                }

                if (
                    (!submittedAnswer.Answer || !question.RequiresAdditionalText)
                    && !string.IsNullOrWhiteSpace(submittedAnswer.AdditionalText)
                )
                {
                    throw new BadRequestException(
                        $"Question '{question.TextEn}' should not contain additional text.",
                        ErrorCodes.AdditionalTextNotAllowed
                    );
                }
            }
        }

        private ScreeningEvaluationResult EvaluateScreeningAnswers(
            List<ScreeningAnswerDTO> submittedAnswers,
            List<ScreeningQuestion> activeQuestions
        )
        {
            var questionMap = activeQuestions.ToDictionary(q => q.Id, q => q);

            var answeredYesQuestions = submittedAnswers
                .Where(a => a.Answer && questionMap.ContainsKey(a.ScreeningQuestionId))
                .Select(a => questionMap[a.ScreeningQuestionId])
                .ToList();

            var autoDeferralQuestions = answeredYesQuestions
                .Where(q => q.DecisionMode == ScreeningDecisionMode.AutoDeferralWhenYes)
                .ToList();

            var hasReviewAnswers = answeredYesQuestions.Any(q =>
                q.DecisionMode == ScreeningDecisionMode.ReviewWhenYes
            );

            if (autoDeferralQuestions.Any(q => q.DeferralType == DeferralType.Permanent))
            {
                return new ScreeningEvaluationResult
                {
                    ResultEligibilityStatus = EligibilityStatus.PermDeferred,
                    HasReviewAnswers = hasReviewAnswers,
                    NextEligibleDate = null,
                    PermanentDeferralReason = autoDeferralQuestions
                        .First(q => q.DeferralType == DeferralType.Permanent)
                        .TextEn
                };
            }

            var temporaryQuestions = autoDeferralQuestions
                .Where(q => q.DeferralType == DeferralType.Temporary)
                .ToList();

            if (temporaryQuestions.Any())
            {
                var nextEligibleDate = CalculateLatestTemporaryDeferralEndDate(
                    submittedAnswers,
                    temporaryQuestions
                );

                return new ScreeningEvaluationResult
                {
                    ResultEligibilityStatus = EligibilityStatus.TempDeferred,
                    HasReviewAnswers = hasReviewAnswers,
                    NextEligibleDate = nextEligibleDate
                };
            }

            return new ScreeningEvaluationResult
            {
                ResultEligibilityStatus = EligibilityStatus.Eligible,
                HasReviewAnswers = hasReviewAnswers,
                NextEligibleDate = null
            };
        }

        private DateTime? CalculateLatestTemporaryDeferralEndDate(
            List<ScreeningAnswerDTO> submittedAnswers,
            List<ScreeningQuestion> temporaryQuestions
        )
        {
            var temporaryQuestionMap = temporaryQuestions.ToDictionary(q => q.Id, q => q);
            DateTime? latestEndDate = null;

            foreach (var answer in submittedAnswers.Where(a => a.Answer))
            {
                if (!temporaryQuestionMap.TryGetValue(answer.ScreeningQuestionId, out var question))
                {
                    continue;
                }

                if (!question.DeferralPeriodDays.HasValue)
                {
                    continue;
                }

                var baseDate = answer.ConditionalDateValue ?? DateTime.UtcNow;
                var endDate = baseDate.AddDays(question.DeferralPeriodDays.Value);

                if (!latestEndDate.HasValue || endDate > latestEndDate.Value)
                {
                    latestEndDate = endDate;
                }
            }

            return latestEndDate;
        }

        private List<ScreeningAnswer> CreateScreeningAnswers(
            int userId,
            int? donorProfileId,
            int screeningSessionId,
            int? donationIntentId,
            List<ScreeningAnswerDTO> submittedAnswers
        )
        {
            return submittedAnswers
                .Select(a => new ScreeningAnswer
                {
                    Answer = a.Answer,
                    CreatedAt = DateTime.UtcNow,
                    ConditionalDateValue = a.ConditionalDateValue,
                    AdditionalText = string.IsNullOrWhiteSpace(a.AdditionalText)
                        ? null
                        : a.AdditionalText.Trim(),
                    UserId = userId,
                    ScreeningSessionId = screeningSessionId,
                    DonationIntentId = donationIntentId,
                    DonorProfileId = donorProfileId,
                    ScreeningQuestionId = a.ScreeningQuestionId,
                })
                .ToList();
        }

        private void ApplyEligibilityToDonorProfile(
            DonorProfile donorProfile,
            ScreeningEvaluationResult evaluation
        )
        {
            donorProfile.EligibilityStatus = evaluation.ResultEligibilityStatus;
            donorProfile.UpdatedAt = DateTime.UtcNow;

            if (evaluation.ResultEligibilityStatus == EligibilityStatus.TempDeferred)
            {
                donorProfile.NextEligibleDate = evaluation.NextEligibleDate;
                donorProfile.PermanentDeferralReason = null;
                return;
            }

            if (evaluation.ResultEligibilityStatus == EligibilityStatus.PermDeferred)
            {
                donorProfile.NextEligibleDate = null;
                donorProfile.PermanentDeferralReason = evaluation.PermanentDeferralReason;
                return;
            }

            if (evaluation.ResultEligibilityStatus == EligibilityStatus.Eligible)
            {
                donorProfile.PermanentDeferralReason = null;

                if (
                    donorProfile.NextEligibleDate.HasValue
                    && donorProfile.NextEligibleDate.Value <= DateTime.UtcNow
                )
                {
                    donorProfile.NextEligibleDate = null;
                }
            }
        }

        private async Task CreateDeferralRecordsIfNeededAsync(
            int? donorProfileId,
            int screeningSessionId,
            List<ScreeningAnswerDTO> submittedAnswers,
            List<ScreeningQuestion> activeQuestions
        )
        {
            if (!donorProfileId.HasValue)
            {
                return;
            }

            var questionMap = activeQuestions.ToDictionary(q => q.Id, q => q);

            var answersThatCauseDeferral = submittedAnswers
                .Where(a =>
                    a.Answer
                    && questionMap.ContainsKey(a.ScreeningQuestionId)
                    && questionMap[a.ScreeningQuestionId].DecisionMode
                        == ScreeningDecisionMode.AutoDeferralWhenYes
                    && questionMap[a.ScreeningQuestionId].DeferralType != DeferralType.NoDeferral
                )
                .ToList();

            foreach (var answer in answersThatCauseDeferral)
            {
                var question = questionMap[answer.ScreeningQuestionId];

                DateTime? startDate = DateTime.UtcNow;
                DateTime? endDate = null;

                if (
                    question.DeferralType == DeferralType.Temporary
                    && question.DeferralPeriodDays.HasValue
                )
                {
                    var baseDate = answer.ConditionalDateValue ?? DateTime.UtcNow;
                    startDate = baseDate;
                    endDate = baseDate.AddDays(question.DeferralPeriodDays.Value);
                }

                var record = new DeferralRecord
                {
                    DonorProfileId = donorProfileId.Value,
                    ScreeningSessionId = screeningSessionId,
                    ScreeningQuestionId = question.Id,
                    DeferralType = question.DeferralType,
                    Reason = question.TextEn,
                    DecisionSource = DecisionSource.SystemAutomatic,
                    CreatedAt = DateTime.UtcNow,
                    StartDate = startDate,
                    EndDate = endDate,
                };

                await _context.DeferralRecords.AddAsync(record);
            }
        }

        private SubmittedScreeningResponseDTO BuildSubmittedScreeningResponse(
            ScreeningSession session,
            bool isProfileCompleted,
            DateTime? nextEligibleDate,
            int savedAnswersCount
        )
        {
            return new SubmittedScreeningResponseDTO
            {
                ScreeningSessionId = session.Id,
                SessionType = session.SessionType,
                IsProfileCompleted = isProfileCompleted,
                ResultEligibilityStatus = session.ResultEligibilityStatus,
                HasReviewAnswers = session.HasReviewAnswers,
                NextEligibleDate = nextEligibleDate,
                CanContinueToDonation =
                    session.SessionType == ScreeningSessionType.PreDonation
                    && session.ResultEligibilityStatus == EligibilityStatus.Eligible,
                CreatedAt = session.CreatedAt,
                SavedAnswersCount = savedAnswersCount,
            };
        }

        public async Task<List<GetScreeningQuestionsResponseDTO>> GetScreeningQuestionsAsync(
            ScreeningSessionType sessionType,
            bool isForFemaleOnly
        )
        {
            var query = _context
                .ScreeningQuestions.AsNoTracking()
                .Where(q => q.IsActive && q.SessionType == sessionType);

            if (!isForFemaleOnly)
            {
                query = query.Where(q => !q.IsForFemaleOnly);
            }

            var questions = await query
                .OrderBy(q => q.DisplayOrder)
                .Select(q => new GetScreeningQuestionsResponseDTO
                {
                    Id = q.Id,
                    TextAr = q.TextAr,
                    TextEn = q.TextEn,
                    SessionType = q.SessionType,
                    DisplayOrder = q.DisplayOrder,
                    IsForFemaleOnly = q.IsForFemaleOnly,
                    RequiresAdditionalText = q.RequiresAdditionalText,
                    RequiresDateValue = q.RequiresDateValue,
                    ConditionalDateLabelAr = q.ConditionalDateLabelAr,
                    ConditionalDateLabelEn = q.ConditionalDateLabelEn,
                    AdditionalTextLabelAr = q.AdditionalTextLabelAr,
                    AdditionalTextLabelEn = q.AdditionalTextLabelEn,
                })
                .ToListAsync();

            return questions;
        }

        private class ScreeningEvaluationResult
        {
            public EligibilityStatus ResultEligibilityStatus { get; set; }
            public bool HasReviewAnswers { get; set; }
            public DateTime? NextEligibleDate { get; set; }
            public string? PermanentDeferralReason { get; set; }
        }
    }
}