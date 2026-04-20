using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QatratHayat.Application.Features.ScreeningQuestions.DTOs;
using QatratHayat.Application.Features.ScreeningQuestions.Interfaces;
using QatratHayat.Domain.Enums;
using System.Security.Claims;

namespace QatratHayat.API.Controllers.ScreeningControllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class ScreeningController : ControllerBase
    {
        private readonly IScreeningSessionService _screeningSessionService;

        public ScreeningController(IScreeningSessionService screeningSessionService)
        {
            _screeningSessionService = screeningSessionService;
        }
        [HttpGet("questions")]
        public async Task<ActionResult<List<GetScreeningQuestionsResponseDTO>>> GetQuestions([FromQuery] ScreeningSessionType sessionType, bool isForFemaleOnly)
        {

            var result = await _screeningSessionService.GetScreeningQuestionsAsync(sessionType, isForFemaleOnly);

            return Ok(result);
        }
        [Authorize]
        [HttpPost("submit")]
        public async Task<ActionResult<SubmittedScreeningResponseDTO>> Submit(SubmittedScreeningQuestionsRequestDTO request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out int userId))
                return Unauthorized();

            var result = await _screeningSessionService.SubmitScreeningQuestionsAsync(userId, request);

            return Ok(result);
        }
    }
}
