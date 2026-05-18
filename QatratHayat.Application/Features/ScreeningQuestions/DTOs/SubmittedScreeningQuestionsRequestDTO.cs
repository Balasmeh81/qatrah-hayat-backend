using QatratHayat.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace QatratHayat.Application.Features.ScreeningQuestions.DTOs
{
    public class SubmittedScreeningQuestionsRequestDTO
    {
        [Required]
        public ScreeningSessionType SessionType { get; set; }

        [Required]
        [MinLength(1)]
        public List<ScreeningAnswerDTO> Answers { get; set; } = new();
    }
}