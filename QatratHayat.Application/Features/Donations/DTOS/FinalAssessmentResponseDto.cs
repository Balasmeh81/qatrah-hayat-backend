using QatratHayat.Domain.Enums;

namespace QatratHayat.Application.Features.Donations.DTOs
{
    public class FinalAssessmentResponseDto
    {
        public int DonationIntentId { get; set; }

        public DonationIntentStatus DonationIntentStatus { get; set; }

        public FinalEligibilityStatus FinalEligibilityStatus { get; set; }

        public int? DonationId { get; set; }
        public int? BloodUnitId { get; set; }

        public string Message { get; set; } = null!;
    }
}