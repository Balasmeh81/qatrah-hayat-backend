using QatratHayat.Domain.Enums;
using System.ComponentModel.DataAnnotations;


namespace QatratHayat.Domain.Entities
{
    public class ScreeningAnswer
    {
        public int Id { get; set; }
        [Required]
        public SessionType SessionType { get; set; }
        [Required]
        public bool Answer { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }

        public DateTime? ConditionalDateValue { get; set; }

        // Navigation Property
        [Required]
        public int UserId { get; set; }

        public DonationIntent? DonationIntent { get; set; }
        public int? DonationIntentId { get; set; }

        public int? DonorProfileId { get; set; }
        public DonorProfile? DonorProfile { get; set; }

        [Required]
        public int ScreeningQuestionId { get; set; }
        public ScreeningQuestion ScreeningQuestion { get; set; } = null!;

    }
}
