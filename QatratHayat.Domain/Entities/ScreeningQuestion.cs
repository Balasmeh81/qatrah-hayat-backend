using QatratHayat.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace QatratHayat.Domain.Entities
{
    public class ScreeningQuestion
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(1000)]
        public string TextAr { get; set; } = null!;
        [Required]
        [MaxLength(1000)]
        public string TextEn { get; set; } = null!;
        [Required]
        public ScreeningLevel ScreeningLevel { get; set; }
        [Required]
        public DeferralType DeferralType { get; set; }      
        [Required]
        public bool IsConditional { get; set; }
        [Required]
        public bool IsActive { get; set; }  
        [Required]
        public int DisplayOrder { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }

        [MaxLength(200)]
        public string? ConditionalDateLabel { get; set; }
        public int? DeferralPeriodDays { get; set; }

        // Navigation Property
        public ICollection<ScreeningAnswer> ScreeningAnswers { get; set; } = new List<ScreeningAnswer>();

    }
}
