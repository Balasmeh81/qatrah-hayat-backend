using QatratHayat.Domain.Enums;

namespace QatratHayat.Application.Features.Donations.DTOs
{
    public class DonationEligibilityResponseDto
    {
        public bool CanDonate { get; set; }
        public bool RequiresRegistrationScreening { get; set; }
        public bool HasActiveIntent { get; set; }

        public int? ActiveIntentId { get; set; }

        public EligibilityStatus? EligibilityStatus { get; set; }
        public DateTime? NextEligibleDate { get; set; }

        public string? Reason { get; set; }
        public string? ErrorCode { get; set; }
    }
}