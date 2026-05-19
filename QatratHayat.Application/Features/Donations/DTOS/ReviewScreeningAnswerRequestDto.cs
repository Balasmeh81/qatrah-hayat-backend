using System.ComponentModel.DataAnnotations;

namespace QatratHayat.Application.Features.Donations.DTOs
{
    public class ReviewScreeningAnswerRequestDto
    {
        [Required]
        public int AnswerId { get; set; }

        [Required]
        public bool ReviewedAnswer { get; set; }

        public DateTime? ReviewedConditionalDateValue { get; set; }

        [MaxLength(1000)]
        public string? ReviewedAdditionalText { get; set; }

        [MaxLength(1000)]
        public string? EmployeeReviewNotes { get; set; }
    }
}