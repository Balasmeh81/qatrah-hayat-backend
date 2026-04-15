using QatratHayat.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace QatratHayat.Domain.Entities
{
    public class DeferralRecord
    {
        public int Id { get; set; }

        [Required]
        public DeferralType DeferralType { get; set; }
        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = null!;
        [Required]
        public DecisionSource DecisionSource { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }


        // Navigation Property
        [Required]
        public int DonorProfileId { get; set; }
        public DonorProfile DonorProfile { get; set; } = null!;

        public int? ScreeningQuestionId { get; set; }
        public ScreeningQuestion? ScreeningQuestion { get; set; }

        public int? ScreeningSessionId { get; set; }
        public ScreeningSession? ScreeningSession { get; set; }

        public int? DecidedByUserId { get; set; }


    }
}
