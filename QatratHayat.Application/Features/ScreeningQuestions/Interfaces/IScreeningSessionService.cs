using QatratHayat.Application.Features.ScreeningQuestions.DTOs;
using QatratHayat.Domain.Enums;

namespace QatratHayat.Application.Features.ScreeningQuestions.Interfaces
{
    public interface IScreeningSessionService
    {
        Task<SubmitedScreeningResponseDTO> SubmitScreeningQuestionsAsync(
            int userId,
            SubmitedScreeningQuestionsRequestDTO request);

        Task<List<GetScreeningQuestionsResponseDTO>> GetScreeningQuestionsAsync(
            ScreeningSessionType sessionType, bool isForFemaleOnly);
    }
}
