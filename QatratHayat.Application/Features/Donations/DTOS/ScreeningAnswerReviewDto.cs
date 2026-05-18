namespace QatratHayat.Application.Features.Donations.DTOs
{
    public class ScreeningAnswerReviewDto
    {
        public int AnswerId { get; set; }
        public int ScreeningQuestionId { get; set; }

        public string QuestionTextAr { get; set; } = null!;
        public string QuestionTextEn { get; set; } = null!;

        public bool Answer { get; set; }

        public DateTime? ConditionalDateValue { get; set; }
        public string? AdditionalText { get; set; }

        public bool RequiresReview { get; set; }
    }
}