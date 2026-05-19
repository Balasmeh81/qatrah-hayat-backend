using QatratHayat.Application.Common.DTOS;
using QatratHayat.Application.Features.Donations.DTOs;

namespace QatratHayat.Application.Features.Donations.Interfaces
{
    public interface IDonationService
    {
        // Citizen
        Task<DonationEligibilityResponseDto> GetDonationEligibilityAsync(int userId);

        Task<PagedResultDto<PublishedBloodRequestForDonationDto>> GetPublishedRequestsAsync(
            int userId,
            PublishedBloodRequestsForDonationQueryDto query
        );
        Task<PublishedBloodRequestForDonationDto> GetPublishedRequestByIdAsync(
            int userId,
            int bloodRequestId
        );

        Task<DonationIntentResponseDto> CreateGeneralDonationIntentAsync(
            int userId,
            CreateGeneralDonationIntentRequestDto request
        );

        Task<DonationIntentResponseDto> CreateRequestDonationIntentAsync(
            int userId,
            CreateRequestDonationIntentRequestDto request
        );

        Task<List<DonationIntentResponseDto>> GetMyDonationIntentsAsync(int userId);

        Task<DonationIntentResponseDto> GetMyDonationIntentByIdAsync(int userId, int intentId);

        Task<DonationIntentResponseDto> CancelMyDonationIntentAsync(int userId, int intentId);

        // Employee
        Task<List<BranchDonationIntentListItemDto>> GetBranchDonationIntentsAsync(
            int employeeUserId,
            BranchDonationIntentQueryDto query
        );

        Task<BranchDonationIntentDetailsDto> ReviewBranchIntentScreeningAsync(
            int employeeUserId,
            int intentId,
            ReviewScreeningAnswersRequestDto request
        );
        Task<BranchDonationIntentDetailsDto> GetBranchDonationIntentDetailsAsync(
            int employeeUserId,
            int intentId
        );

        Task<FinalAssessmentResponseDto> SubmitFinalAssessmentAsync(
            int employeeUserId,
            int intentId,
            FinalAssessmentRequestDto request
        );
    }
}
