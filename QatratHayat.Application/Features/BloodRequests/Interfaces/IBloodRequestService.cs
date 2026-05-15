using QatratHayat.Application.Common.DTOS;
using QatratHayat.Application.Features.BloodRequests.DTOS;

namespace QatratHayat.Application.Features.BloodRequests.Interfaces
{
    public interface IBloodRequestService
    {
        Task<CitizenDataResponseDto> GetCurrentCitizenDataAsync();

        Task<CitizenDataResponseDto> LookupBeneficiaryByNationalIdAsync(
            string nationalId
        );
        Task<BloodRequestDetailsResponseDto> CreateAsync(
            CreateBloodRequestDto dto
        );

        Task<PagedResultDto<BloodRequestResponseDto>> GetMyRequestsAsync(
            BloodRequestQueryDto query
        );

        Task<PagedResultDto<BloodRequestResponseDto>> GetDoctorRequestsAsync(
            BloodRequestQueryDto query
        );

        Task<PagedResultDto<BloodRequestResponseDto>> GetBranchRequestsAsync(
            BloodRequestQueryDto query
        );

        Task<BloodRequestDetailsResponseDto> GetByIdAsync(
            int requestId
        );

        Task<BloodRequestDetailsResponseDto> DoctorReviewAsync(
            int requestId,
            DoctorReviewBloodRequestRequestDto dto
        );

        Task<BloodRequestDetailsResponseDto> EmployeeReviewAsync(
            int requestId,
            EmployeeReviewBloodRequestRequestDto dto
        );

        Task<BloodRequestDetailsResponseDto> ConfirmAllocationAsync(
            int requestId,
            ConfirmBloodRequestAllocationRequestDto dto
        );

        Task<BloodRequestDetailsResponseDto> PublishAsync(
            int requestId
        );

        Task<BloodRequestDetailsResponseDto> RejectAsync(
            int requestId,
            RejectBloodRequestRequestDto dto
        );

        Task<BloodRequestDetailsResponseDto> CancelAsync(
            int requestId,
            CancelBloodRequestDto dto
        );
    }
}