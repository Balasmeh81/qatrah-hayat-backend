using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QatratHayat.Application.Features.Donations.DTOs;
using QatratHayat.Application.Features.Donations.Interfaces;
using System.Security.Claims;

namespace QatratHayat.API.Controllers.DonationControllers
{
    [Route("api/donations")]
    [ApiController]
    [Authorize(Roles = "Employee,BranchManager,Admin")]
    public class EmployeeDonationController : ControllerBase
    {
        private readonly IDonationService _donationService;

        public EmployeeDonationController(IDonationService donationService)
        {
            _donationService = donationService;
        }

        [HttpGet("branch-intents")]
        public async Task<ActionResult<List<BranchDonationIntentListItemDto>>> GetBranchIntents(
            [FromQuery] BranchDonationIntentQueryDto query
        )
        {
            var employeeUserId = GetCurrentUserId();

            var result = await _donationService.GetBranchDonationIntentsAsync(
                employeeUserId,
                query
            );

            return Ok(result);
        }

        [HttpGet("branch-intents/{id:int}")]
        public async Task<ActionResult<BranchDonationIntentDetailsDto>> GetBranchIntentDetails(
            int id
        )
        {
            var employeeUserId = GetCurrentUserId();

            var result = await _donationService.GetBranchDonationIntentDetailsAsync(
                employeeUserId,
                id
            );

            return Ok(result);
        }

        [HttpPost("branch-intents/{id:int}/final-assessment")]
        public async Task<ActionResult<FinalAssessmentResponseDto>> SubmitFinalAssessment(
            int id,
            FinalAssessmentRequestDto request
        )
        {
            var employeeUserId = GetCurrentUserId();

            var result = await _donationService.SubmitFinalAssessmentAsync(
                employeeUserId,
                id,
                request
            );

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