using QatratHayat.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace QatratHayat.Domain.Entities
{
    public class DonationIntent
    {
        public int Id { get; set; }
        [Required]
        public DonationType DonationType { get; set; }
        [Required]
        public DonationIntentStatus DonationIntentStatus { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }
        [Required]
        public DateTime ExpiresAt { get; set; }

        // Navigation Property
        [Required]
        public int DonorProfileId { get; set; }
        public DonorProfile DonorProfile { get; set; } = null!;

        public int? CampaignId { get; set; }
        public Campaign? Campaign { get; set; }

        public int? BloodRequestId { get; set; }
        public BloodRequest? BloodRequest { get; set; }

        public ICollection<ScreeningAnswer> ScreeningAnswers { get; set; } = new List<ScreeningAnswer>();

    }
}
