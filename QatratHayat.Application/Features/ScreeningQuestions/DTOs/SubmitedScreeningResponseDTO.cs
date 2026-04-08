using QatratHayat.Domain.Enums;

namespace QatratHayat.Application.Features.ScreeningQuestions.DTOs
{
    public class SubmitedScreeningResponseDTO
    {
        public int ScreeningSessionId { get; set; }
        public ScreeningSessionType SessionType { get; set; }
        public bool IsProfileCompleted { get; set; }
        public EligibilityStatus ResultEligibilityStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public int SavedAnswersCount { get; set; }
    }
}
