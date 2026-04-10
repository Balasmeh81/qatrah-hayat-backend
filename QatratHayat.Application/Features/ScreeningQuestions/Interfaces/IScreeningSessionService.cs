using QatratHayat.Application.Features.ScreeningQuestions.DTOs;
using QatratHayat.Domain.Enums;

namespace QatratHayat.Application.Features.ScreeningQuestions.Interfaces
{
    public interface IScreeningSessionService
    {
        Task<SubmittedScreeningResponseDTO> SubmitScreeningQuestionsAsync(
            int userId,
            SubmittedScreeningQuestionsRequestDTO request);

        Task<List<GetScreeningQuestionsResponseDTO>> GetScreeningQuestionsAsync(
            ScreeningSessionType sessionType, bool isForFemaleOnly);
    }
}
