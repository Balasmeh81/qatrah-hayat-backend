using System.ComponentModel.DataAnnotations;

namespace QatratHayat.Application.Features.ScreeningQuestions.DTOs
{
    public class ScreeningAnswerDTO
    {
        [Required]
        public int ScreeningQuestionId { get; set; }

        [Required]
        public bool Answer { get; set; }

        public DateTime? ConditionalDateValue { get; set; }

        [MaxLength(1000)]
        public string? AdditionalText { get; set; }
    }
}
