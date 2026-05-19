using QatratHayat.Domain.Enums;

namespace QatratHayat.Application.Features.Donations.DTOs
{
    public class BranchDonationIntentListItemDto
    {
        public int Id { get; set; }

        public DonationType DonationType { get; set; }
        public DonationIntentStatus DonationIntentStatus { get; set; }
        public bool HasUnreviewedRequiredAnswers { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }

        public int DonorProfileId { get; set; }
        public int DonorUserId { get; set; }

        public string NationalId { get; set; } = null!;
        public string FullNameAr { get; set; } = null!;
        public string FullNameEn { get; set; } = null!;

        public BloodType BloodType { get; set; }
        public BloodTypeStatus BloodTypeStatus { get; set; }
        public EligibilityStatus EligibilityStatus { get; set; }

        public int BranchId { get; set; }
        public string BranchNameAr { get; set; } = null!;
        public string BranchNameEn { get; set; } = null!;

        public int? BloodRequestId { get; set; }
        public int? CampaignId { get; set; }

        public bool HasReviewAnswers { get; set; }
    }
}