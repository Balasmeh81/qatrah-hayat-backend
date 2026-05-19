using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QatratHayat.Application.Common.DTOS;
using QatratHayat.Application.Features.Donations.DTOs;
using QatratHayat.Application.Features.Donations.Interfaces;
using System.Security.Claims;

namespace QatratHayat.API.Controllers.DonationControllers
{
    [Route("api/donations")]
    [ApiController]
    [Authorize(Roles = "Citizen")]
    public class DonationController : ControllerBase
    {
        private readonly IDonationService _donationService;

        public DonationController(IDonationService donationService)
        {
            _donationService = donationService;
        }

        [HttpGet("eligibility")]
        public async Task<ActionResult<DonationEligibilityResponseDto>> GetEligibility()
        {
            var userId = GetCurrentUserId();

            var result = await _donationService.GetDonationEligibilityAsync(userId);

            return Ok(result);
        }

        [HttpGet("published-requests")]
        public async Task<
            ActionResult<PagedResultDto<PublishedBloodRequestForDonationDto>>
        > GetPublishedRequests([FromQuery] PublishedBloodRequestsForDonationQueryDto query)
        {
            var userId = GetCurrentUserId();

            var result = await _donationService.GetPublishedRequestsAsync(userId, query);

            return Ok(result);
        }

        [HttpGet("published-requests/{id:int}")]
        public async Task<
            ActionResult<PublishedBloodRequestForDonationDto>
        > GetPublishedRequestById(int id)
        {
            var userId = GetCurrentUserId();

            var result = await _donationService.GetPublishedRequestByIdAsync(userId, id);

            return Ok(result);
        }

        [HttpPost("intents/general")]
        public async Task<ActionResult<DonationIntentResponseDto>> CreateGeneralIntent(
            CreateGeneralDonationIntentRequestDto request
        )
        {
            var userId = GetCurrentUserId();

            var result = await _donationService.CreateGeneralDonationIntentAsync(userId, request);

            return Ok(result);
        }

        [HttpPost("intents/request")]
        public async Task<ActionResult<DonationIntentResponseDto>> CreateRequestIntent(
            CreateRequestDonationIntentRequestDto request
        )
        {
            var userId = GetCurrentUserId();

            var result = await _donationService.CreateRequestDonationIntentAsync(userId, request);

            return Ok(result);
        }

        [HttpGet("my-intents")]
        public async Task<ActionResult<List<DonationIntentResponseDto>>> GetMyIntents()
        {
            var userId = GetCurrentUserId();

            var result = await _donationService.GetMyDonationIntentsAsync(userId);

            return Ok(result);
        }

        [HttpGet("my-intents/{id:int}")]
        public async Task<ActionResult<DonationIntentResponseDto>> GetMyIntentById(int id)
        {
            var userId = GetCurrentUserId();

            var result = await _donationService.GetMyDonationIntentByIdAsync(userId, id);

            return Ok(result);
        }

        [HttpPost("my-intents/{id:int}/cancel")]
        public async Task<ActionResult<DonationIntentResponseDto>> CancelMyIntent(int id)
        {
            var userId = GetCurrentUserId();

            var result = await _donationService.CancelMyDonationIntentAsync(userId, id);

            return Ok(result);
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException("Invalid user id claim.");
            }

            return userId;
        }
    }
}
