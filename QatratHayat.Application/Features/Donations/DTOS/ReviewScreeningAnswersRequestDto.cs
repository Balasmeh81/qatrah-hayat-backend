using System.ComponentModel.DataAnnotations;

namespace QatratHayat.Application.Features.Donations.DTOs
{
    public class ReviewScreeningAnswersRequestDto
    {
        [Required]
        [MinLength(1)]
        public List<ReviewScreeningAnswerRequestDto> Answers { get; set; } = [];
    }
}