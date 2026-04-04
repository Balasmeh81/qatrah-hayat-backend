using QatratHayat.Domain.Enums;

namespace QatratHayat.Domain.Entities
{
    public class DonorProfile
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public BloodType BloodType { get; set; }

        public BloodTypeStatus BloodTypeStatus { get; set; } = BloodTypeStatus.Provisional;

        public int? BloodTypeConfirmedByEmployeeId { get; set; }

        public DateTime? BloodTypeConfirmedAt { get; set; }

        public EligibilityStatus EligibilityStatus { get; set; } = EligibilityStatus.Eligible;

        public string? PermanentDeferralReason { get; set; }

        public DateTime? LastDonationDate { get; set; }

        public DateTime? NextEligibleDate { get; set; }

        public int DonationCount { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}