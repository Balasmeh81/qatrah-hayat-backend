using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QatratHayat.Application.Common.DTOS;
using QatratHayat.Application.Features.Campaigns.DTOS;
using QatratHayat.Application.Features.Campaigns.Interfaces;
using QatratHayat.Domain.Enums;
using System.Security.Claims;

namespace QatratHayat.API.Controllers.Admin
{
    [ApiController]
    [Route("api/campaigns")]
    [Authorize(Roles = "Admin,BranchManager")]
    public class CampaignsController : ControllerBase
    {
        private readonly ICampaignService _campaignService;

        public CampaignsController(ICampaignService campaignService)
        {
            _campaignService = campaignService;
        }

        // ============================================================
        // Get All Campaigns
        // ============================================================

        [HttpGet]
        public async Task<ActionResult<PagedResultDto<CampaignResponseDto>>> GetAllCampaigns(
            [FromQuery] CampaignQueryDto query
        )
        {
            if (!TryGetCurrentUserId(out var currentUserId))
            {
                return Unauthorized();
            }

            var result = await _campaignService.GetAllCampaignsAsync(
                query,
                currentUserId,
                IsAdmin()
            );

            return Ok(result);
        }

        // ============================================================
        // Get Statistics
        // ============================================================

        [HttpGet("statistics")]
        public async Task<ActionResult<CampaignStatisticsResponseDto>> GetStatistics()
        {
            if (!TryGetCurrentUserId(out var currentUserId))
            {
                return Unauthorized();
            }

            var result = await _campaignService.GetStatisticsAsync(
                currentUserId,
                IsAdmin()
            );

            return Ok(result);
        }

        // ============================================================
        // Get Campaign By Id
        // ============================================================

        [HttpGet("{campaignId:int}")]
        public async Task<ActionResult<CampaignResponseDto>> GetCampaignById(
            [FromRoute] int campaignId
        )
        {
            if (!TryGetCurrentUserId(out var currentUserId))
            {
                return Unauthorized();
            }

            var result = await _campaignService.GetCampaignByIdAsync(
                campaignId,
                currentUserId,
                IsAdmin()
            );

            return Ok(result);
        }

        // ============================================================
        // Create Campaign
        // ============================================================

        [HttpPost]
        public async Task<ActionResult<CampaignResponseDto>> CreateCampaign(
            [FromBody] CreateCampaignRequestDto request
        )
        {
            if (!TryGetCurrentUserId(out var createdByUserId))
            {
                return Unauthorized();
            }

            var result = await _campaignService.CreateCampaignAsync(
                request,
                createdByUserId,
                IsAdmin()
            );

            return CreatedAtAction(
                nameof(GetCampaignById),
                new { campaignId = result.Id },
                result
            );
        }

        // ============================================================
        // Update Campaign
        // ============================================================

        [HttpPut("{campaignId:int}")]
        public async Task<ActionResult<CampaignResponseDto>> UpdateCampaign(
            [FromRoute] int campaignId,
            [FromBody] UpdateCampaignRequestDto request
        )
        {
            if (!TryGetCurrentUserId(out var currentUserId))
            {
                return Unauthorized();
            }

            var result = await _campaignService.UpdateCampaignAsync(
                campaignId,
                request,
                currentUserId,
                IsAdmin()
            );

            return Ok(result);
        }

        // ============================================================
        // Soft Delete Campaign
        // ============================================================

        [HttpDelete("{campaignId:int}")]
        public async Task<IActionResult> SoftDeleteCampaign(
            [FromRoute] int campaignId
        )
        {
            if (!TryGetCurrentUserId(out var currentUserId))
            {
                return Unauthorized();
            }

            await _campaignService.SoftDeleteCampaignAsync(
                campaignId,
                currentUserId,
                IsAdmin()
            );

            return NoContent();
        }

        // ============================================================
        // Activate Campaign
        // ============================================================

        [HttpPatch("{campaignId:int}/activate")]
        public async Task<IActionResult> ActivateCampaign(
            [FromRoute] int campaignId
        )
        {
            if (!TryGetCurrentUserId(out var currentUserId))
            {
                return Unauthorized();
            }

            await _campaignService.ActivateCampaignAsync(
                campaignId,
                currentUserId,
                IsAdmin()
            );

            return NoContent();
        }

        // ============================================================
        // Complete Campaign
        // ============================================================

        [HttpPatch("{campaignId:int}/complete")]
        public async Task<IActionResult> CompleteCampaign(
            [FromRoute] int campaignId
        )
        {
            if (!TryGetCurrentUserId(out var currentUserId))
            {
                return Unauthorized();
            }

            await _campaignService.CompleteCampaignAsync(
                campaignId,
                currentUserId,
                IsAdmin()
            );

            return NoContent();
        }

        // ============================================================
        // Cancel Campaign
        // ============================================================

        [HttpPatch("{campaignId:int}/cancel")]
        public async Task<IActionResult> CancelCampaign(
            [FromRoute] int campaignId
        )
        {
            if (!TryGetCurrentUserId(out var currentUserId))
            {
                return Unauthorized();
            }

            await _campaignService.CancelCampaignAsync(
                campaignId,
                currentUserId,
                IsAdmin()
            );

            return NoContent();
        }

        // ============================================================
        // Helpers
        // ============================================================

        private bool TryGetCurrentUserId(out int userId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return int.TryParse(userIdClaim, out userId);
        }

        private bool IsAdmin()
        {
            return User.IsInRole(UserRole.Admin.ToString());
        }
    }
}