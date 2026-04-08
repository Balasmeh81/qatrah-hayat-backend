using QatratHayat.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace QatratHayat.Domain.Entities
{
    public class ScreeningSession
    {
        public int Id { get; set; }

        [Required]
        public ScreeningSessionType SessionType { get; set; }

        [Required]
        public EligibilityStatus ResultEligibilityStatus { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        [Required]
        public int UserId { get; set; }

        public int? DonorProfileId { get; set; }
        public DonorProfile? DonorProfile { get; set; }

        public int? DonationIntentId { get; set; }
        public DonationIntent? DonationIntent { get; set; }

        public ICollection<ScreeningAnswer> Answers { get; set; } = new List<ScreeningAnswer>();
    }
}
