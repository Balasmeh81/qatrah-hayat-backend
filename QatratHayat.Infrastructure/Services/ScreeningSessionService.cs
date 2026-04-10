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

        public async Task<SubmittedScreeningResponseDTO> SubmitScreeningQuestionsAsync(int userId, SubmittedScreeningQuestionsRequestDTO request)
        {
            // 1.Get User
            var user = await _context.Users
                .Include(x => x.DonorProfile)
                .FirstOrDefaultAsync(x => x.Id == userId);
            // 2.Check User
            if (user is null)
                throw new NotFoundException("User not found.");
            if (!user.IsActive || user.IsDeleted)
                throw new UnauthorizedException("This account is inactive.");

            // 3. If Screening Session Type == Registration
            if (request.SessionType == ScreeningSessionType.Registration)
                return await HandleRegistrationAsync(user, request);

            // 4. If Screening Session Type == PreDonation
            if (request.SessionType == ScreeningSessionType.PreDonation)
                return await HandlePreDonationAsync(user, request);

            throw new BadRequestException("Unsupported screening session type.");

        }

        // Handle Registration Questions
        private async Task<SubmittedScreeningResponseDTO> HandleRegistrationAsync(
            Identity.ApplicationUser user,
            SubmittedScreeningQuestionsRequestDTO request)
        {
            // 1. Check if Screening Questions Request For Registration
            if (request.DonationIntentId.HasValue)
                throw new BadRequestException("DonationIntentId must be null for Registration screening.");

            if (user.IsProfileCompleted)
                throw new BadRequestException("Registration screening already completed.");

            // 2. Get All Registration Screening Questions
            var activeQuestions = await _context.ScreeningQuestions
             .Where(q => q.IsActive && q.SessionType == ScreeningSessionType.Registration)
             .OrderBy(q => q.DisplayOrder)
             .ToListAsync();

            // 3. Validate Questions And Answers
            ValidateQuestionsAndAnswers(user, request, activeQuestions);

            // 4. Create Screening Session
            var session = new ScreeningSession
            {
                SessionType = ScreeningSessionType.Registration,
                UserId = user.Id,
                DonorProfileId = user.DonorProfile?.Id ?? null,
                DonationIntentId = request.DonationIntentId ?? null,
                ResultEligibilityStatus = EligibilityStatus.Eligible,
                CreatedAt = DateTime.UtcNow,
                CompletedAt = DateTime.UtcNow
            };
            // 5. Save Screening Session
            _context.ScreeningSessions.Add(session);
            await _context.SaveChangesAsync();

            // 6. Create Screening Answers
            var answers = request.Answers.Select(a => new ScreeningAnswer
            {
                Answer = a.Answer,
                CreatedAt = DateTime.UtcNow,
                ConditionalDateValue = a.ConditionalDateValue,
                AdditionalText = string.IsNullOrWhiteSpace(a.AdditionalText) ? null : a.AdditionalText.Trim(),
                UserId = user.Id,
                ScreeningSessionId = session.Id,
                DonationIntentId = request.DonationIntentId ?? null,
                DonorProfileId = user.DonorProfile?.Id ?? null,
                ScreeningQuestionId = a.ScreeningQuestionId
            }).ToList();

            // 7. Insetrt Screening Answers
            _context.ScreeningAnswers.AddRange(answers);

            // 8. Update Is Profile Completed In User and Save The change in DB
            user.IsProfileCompleted = true;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // 9. Return Screening Response
            return new SubmittedScreeningResponseDTO
            {
                ScreeningSessionId = session.Id,
                SessionType = session.SessionType,
                IsProfileCompleted = user.IsProfileCompleted,
                ResultEligibilityStatus = session.ResultEligibilityStatus,
                CreatedAt = session.CreatedAt,
                SavedAnswersCount = answers.Count
            };
        }

        private async Task<SubmittedScreeningResponseDTO> HandlePreDonationAsync(
            Identity.ApplicationUser user,
            SubmittedScreeningQuestionsRequestDTO request)
        {
            // 1. Check if Screening Questions Request For PreDonation
            if (!request.DonationIntentId.HasValue)
                throw new BadRequestException("DonationIntentId is required for PreDonation screening.");

            // 2. Check if Donor Profile

            if (user.DonorProfile is null)
                throw new BadRequestException("Donor profile is required for PreDonation screening.");

            // 3. Get The Donation Intent
            var donationIntent = await _context.DonationIntents
                .FirstOrDefaultAsync(x => x.Id == request.DonationIntentId.Value);

            // 4. Check The Donation Intent
            if (donationIntent is null)
                throw new NotFoundException("Donation intent not found.");

            if (donationIntent.DonorProfileId != user.DonorProfile.Id)
                throw new UnauthorizedException("Donation intent does not belong to the current user.");

            // 5. Get All PreDonation Screening Questions
            var activeQuestions = await _context.ScreeningQuestions
                .Where(q => q.IsActive && q.SessionType == ScreeningSessionType.PreDonation)
                .OrderBy(q => q.DisplayOrder)
                .ToListAsync();

            // 6. Validate Questions And Answers
            ValidateQuestionsAndAnswers(user, request, activeQuestions);

            // 7. Get The Eligibility Status
            var resultEligibilityStatus = CalculatePreDonationEligibility(request, activeQuestions);

            // 8. Create Screening Session
            var session = new ScreeningSession
            {
                SessionType = ScreeningSessionType.PreDonation,
                UserId = user.Id,
                DonorProfileId = user.DonorProfile.Id,
                DonationIntentId = request.DonationIntentId,
                ResultEligibilityStatus = resultEligibilityStatus,
                CreatedAt = DateTime.UtcNow,
                CompletedAt = DateTime.UtcNow
            };
            // 9. Save Screening Session
            _context.ScreeningSessions.Add(session);
            await _context.SaveChangesAsync();

            // 10. Create Screening Answers
            var answers = request.Answers.Select(a => new ScreeningAnswer
            {
                Answer = a.Answer,
                CreatedAt = DateTime.UtcNow,
                ConditionalDateValue = a.ConditionalDateValue,
                AdditionalText = string.IsNullOrWhiteSpace(a.AdditionalText) ? null : a.AdditionalText.Trim(),
                UserId = user.Id,
                ScreeningSessionId = session.Id,
                DonationIntentId = request.DonationIntentId,
                DonorProfileId = user.DonorProfile.Id,
                ScreeningQuestionId = a.ScreeningQuestionId
            }).ToList();

            // 11. Insert Screening Answers
            _context.ScreeningAnswers.AddRange(answers);

            // 12. Update Eligibility Status in Donor Profile
            user.DonorProfile.EligibilityStatus = resultEligibilityStatus;
            user.DonorProfile.UpdatedAt = DateTime.UtcNow;

            // 13. Create Deferral Records If Needed 
            await CreateDeferralRecordsIfNeededAsync(
                user.DonorProfile.Id,
                session.Id,
                request.Answers,
                activeQuestions);

            // 14. Save the Deferral Records
            await _context.SaveChangesAsync();

            // 13. Return Screening Response
            return new SubmittedScreeningResponseDTO
            {
                ScreeningSessionId = session.Id,
                SessionType = session.SessionType,
                IsProfileCompleted = user.IsProfileCompleted,
                ResultEligibilityStatus = session.ResultEligibilityStatus,
                CreatedAt = session.CreatedAt,
                SavedAnswersCount = answers.Count
            };
        }



        private void ValidateQuestionsAndAnswers(
           Identity.ApplicationUser user,
          SubmittedScreeningQuestionsRequestDTO request,
           List<ScreeningQuestion> activeQuestions)
        {
            if (!activeQuestions.Any())
                throw new Exception("No active screening questions found.");

            var activeQuestionIds = activeQuestions.Select(q => q.Id).ToHashSet();
            var submittedQuestionIds = request.Answers.Select(a => a.ScreeningQuestionId).ToList();

            if (submittedQuestionIds.Count != submittedQuestionIds.Distinct().Count())
                throw new Exception("Duplicate question answers are not allowed.");

            var invalidQuestionIds = submittedQuestionIds
                .Where(id => !activeQuestionIds.Contains(id))
                .Distinct()
                .ToList();

            if (invalidQuestionIds.Any())
                throw new Exception($"Invalid question ids: {string.Join(", ", invalidQuestionIds)}");

            var missingQuestionIds = activeQuestionIds
                .Where(id => !submittedQuestionIds.Contains(id))
                .ToList();

            if (missingQuestionIds.Any())
                throw new Exception($"All active questions must be answered. Missing ids: {string.Join(", ", missingQuestionIds)}");

            foreach (var submittedAnswer in request.Answers)
            {
                var question = activeQuestions.First(q => q.Id == submittedAnswer.ScreeningQuestionId);

                if (question.IsForFemaleOnly && user.Gender != Gender.Female)
                    throw new Exception($"Question '{question.TextEn}' is for female users only.");

                if (submittedAnswer.Answer && question.RequiresDateValue && !submittedAnswer.ConditionalDateValue.HasValue)
                    throw new BadRequestException($"Question '{question.TextEn}' requires a date value.");

                if (!submittedAnswer.Answer && submittedAnswer.ConditionalDateValue.HasValue)
                    throw new BadRequestException($"Question '{question.TextEn}' should not contain a date value.");

                if (submittedAnswer.Answer && question.RequiresAdditionalText && string.IsNullOrWhiteSpace(submittedAnswer.AdditionalText))
                    throw new BadRequestException($"Question '{question.TextEn}' requires additional text.");

                if ((!submittedAnswer.Answer || !question.RequiresAdditionalText) && !string.IsNullOrWhiteSpace(submittedAnswer.AdditionalText))
                    throw new BadRequestException($"Question '{question.TextEn}' should not contain additional text.");
            }
        }



        private EligibilityStatus CalculatePreDonationEligibility(
          SubmittedScreeningQuestionsRequestDTO request,
          List<ScreeningQuestion> activeQuestions)
        {
            var autoDeferralYesQuestions = activeQuestions
                .Where(q => q.DecisionMode == ScreeningDecisionMode.AutoDeferralWhenYes)
                .ToDictionary(q => q.Id, q => q);

            var answeredYes = request.Answers
                .Where(a => a.Answer && autoDeferralYesQuestions.ContainsKey(a.ScreeningQuestionId))
                .Select(a => autoDeferralYesQuestions[a.ScreeningQuestionId])
                .ToList();

            if (!answeredYes.Any())
                return EligibilityStatus.Eligible;

            if (answeredYes.Any(q => q.DeferralType == DeferralType.Permanent))
                return EligibilityStatus.PermDeferred;

            if (answeredYes.Any(q => q.DeferralType == DeferralType.Temporary))
                return EligibilityStatus.TempDeferred;

            return EligibilityStatus.Eligible;
        }

        private async Task CreateDeferralRecordsIfNeededAsync(
            int donorProfileId,
            int screeningSessionId,
            List<ScreeningAnswerDTO> submittedAnswers,
            List<ScreeningQuestion> activeQuestions)
        {
            var questionMap = activeQuestions.ToDictionary(q => q.Id, q => q);

            var answersThatCauseDeferral = submittedAnswers
                .Where(a => a.Answer
                            && questionMap.ContainsKey(a.ScreeningQuestionId)
                            && questionMap[a.ScreeningQuestionId].DecisionMode == ScreeningDecisionMode.AutoDeferralWhenYes
                            && questionMap[a.ScreeningQuestionId].DeferralType != DeferralType.NoDeferral)
                .ToList();

            foreach (var answer in answersThatCauseDeferral)
            {
                var question = questionMap[answer.ScreeningQuestionId];

                DateTime? startDate = DateTime.UtcNow;
                DateTime? endDate = null;

                if (question.DeferralType == DeferralType.Temporary && question.DeferralPeriodDays.HasValue)
                {
                    var baseDate = answer.ConditionalDateValue ?? DateTime.UtcNow;
                    startDate = baseDate;
                    endDate = baseDate.AddDays(question.DeferralPeriodDays.Value);
                }

                var record = new DeferralRecord
                {
                    DonorProfileId = donorProfileId,
                    ScreeningSessionId = screeningSessionId,
                    ScreeningQuestionId = question.Id,
                    DeferralType = question.DeferralType,
                    Reason = question.TextEn,
                    DecisionSource = DecisionSource.SystemAutomatic,
                    CreatedAt = DateTime.UtcNow,
                    StartDate = startDate,
                    EndDate = endDate
                };

                await _context.DeferralRecords.AddAsync(record);
            }
        }

        public async Task<List<GetScreeningQuestionsResponseDTO>> GetScreeningQuestionsAsync(ScreeningSessionType sessionType, bool isForFemaleOnly)
        {
            // 1. Get All Active Screening Questions by Session Type
            var query = _context.ScreeningQuestions
                .AsNoTracking()
                .Where(q => q.IsActive && q.SessionType == sessionType);

            // 2. If The User Male Do Not Get The Screening Questions that for Female
            if (!isForFemaleOnly)
            {
                query = query.Where(q => !q.IsForFemaleOnly);
            }

            // 3. Create List Of GetScreeningQuestionsResponseDTO and Return it
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
                    AdditionalTextLabelEn = q.AdditionalTextLabelEn
                })
                .ToListAsync();

            return questions;
        }
    }
}





