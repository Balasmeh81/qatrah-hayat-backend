using QatratHayat.Domain.Enums;

namespace QatratHayat.Application.Features.Donations.DTOs
{
    public class ScreeningSessionReviewDto
    {
        public int ScreeningSessionId { get; set; }

        public ScreeningSessionType SessionType { get; set; }
        public EligibilityStatus ResultEligibilityStatus { get; set; }

        public bool HasReviewAnswers { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        public List<ScreeningAnswerReviewDto> Answers { get; set; } = new();
    }
}