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

        // Employee review
        public bool? ReviewedAnswer { get; set; }

        public DateTime? ReviewedConditionalDateValue { get; set; }

        public string? ReviewedAdditionalText { get; set; }

        public string? EmployeeReviewNotes { get; set; }

        public int? ReviewedByEmployeeId { get; set; }

        public string? ReviewedByEmployeeNameAr { get; set; }

        public string? ReviewedByEmployeeNameEn { get; set; }

        public DateTime? ReviewedAt { get; set; }
    }
}