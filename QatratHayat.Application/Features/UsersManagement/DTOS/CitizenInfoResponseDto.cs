using QatratHayat.Application.Common.DTOS;
using QatratHayat.Domain.Enums;

namespace QatratHayat.Application.Features.UsersManagement.DTOS
{
    public class CitizenInfoResponseDto : BaseUserInfoDto
    {
        public MaritalStatus MaritalStatus { get; set; }
        public string? JobTitle { get; set; }
        public string? Address { get; set; }
        public BloodTypeStatus BloodTypeStatus { get; set; }
        public EligibilityStatus EligibilityStatus { get; set; }
        public int DonationCount { get; set; }
        public string? PermanentDeferralReason { get; set; }
        public DateTime? LastDonationDate { get; set; }
        public DateTime? NextEligibleDate { get; set; }
        public DateTime? BloodTypeConfirmedAt { get; set; }

    }
}
