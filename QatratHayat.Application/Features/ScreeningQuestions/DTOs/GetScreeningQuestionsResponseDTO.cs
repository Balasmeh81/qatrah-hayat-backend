using QatratHayat.Domain.Enums;

namespace QatratHayat.Application.Features.ScreeningQuestions.DTOs
{
    public class GetScreeningQuestionsResponseDTO
    {

        public int Id { get; set; }
        public string TextAr { get; set; } = null!;
        public string TextEn { get; set; } = null!;
        public ScreeningSessionType SessionType { get; set; }
        public int DisplayOrder { get; set; }

        public bool IsForFemaleOnly { get; set; }
        public bool RequiresAdditionalText { get; set; }
        public bool RequiresDateValue { get; set; }

        public string? ConditionalDateLabelAr { get; set; }
        public string? ConditionalDateLabelEn { get; set; }

        public string? AdditionalTextLabelAr { get; set; }
        public string? AdditionalTextLabelEn { get; set; }
    }
}
