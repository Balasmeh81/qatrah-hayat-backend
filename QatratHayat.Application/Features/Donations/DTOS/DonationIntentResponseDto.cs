using QatratHayat.Domain.Enums;

namespace QatratHayat.Application.Features.Donations.DTOs
{
    public class DonationIntentResponseDto
    {
        public int Id { get; set; }

        public DonationType DonationType { get; set; }
        public DonationIntentStatus DonationIntentStatus { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }

        public int BranchId { get; set; }
        public string BranchNameAr { get; set; } = null!;
        public string BranchNameEn { get; set; } = null!;

        public int? BloodRequestId { get; set; }
        public int? CampaignId { get; set; }

        public bool HasReviewAnswers { get; set; }
        public int? ScreeningSessionId { get; set; }
    }
}