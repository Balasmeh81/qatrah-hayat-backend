using Microsoft.AspNetCore.Mvc;
using QatratHayat.Application.Common.DTOS;
using QatratHayat.Application.Features.BranchManagement.DTOS;
using QatratHayat.Application.Features.BranchManagement.Interfaces;

namespace QatratHayat.API.Controllers.Admin
{
    [ApiController]
    [Route("api/branches-management")]
    //[Authorize(Roles = "Admin")]
    public class BranchManagementController : ControllerBase
    {
        private readonly IBranchManagementService _branchManagementService;

        public BranchManagementController(IBranchManagementService branchManagementService)
        {
            _branchManagementService = branchManagementService;
        }

        // ============================================================
        // Get All Branches
        // ============================================================

        [HttpGet]
        public async Task<ActionResult<PagedResultDto<BranchResponseDto>>> GetAllBranches(
            [FromQuery] BranchQueryDto query)
        {
            var result = await _branchManagementService.GetAllBranchesAsync(query);

            return Ok(result);
        }
        // ============================================================
        // Get Branch Statistics
        // ============================================================

        [HttpGet("statistics")]
        public async Task<ActionResult<BranchStatisticsResponseDto>> GetStatistics()
        {
            var result = await _branchManagementService.GetStatisticsAsync();

            return Ok(result);
        }
        // ============================================================
        // Get Available Branch Managers
        // ============================================================

        [HttpGet("available-managers")]
        public async Task<ActionResult<List<AvailableBranchManagerDto>>> GetAvailableManagers(
            [FromQuery] int? currentBranchId = null)
        {
            var result = await _branchManagementService.GetAvailableManagersAsync(currentBranchId);

            return Ok(result);
        }

        // ============================================================
        // Get Branch By Id
        // ============================================================

        [HttpGet("{branchId:int}")]
        public async Task<ActionResult<BranchResponseDto>> GetBranchById(
            [FromRoute] int branchId)
        {
            var result = await _branchManagementService.GetBranchByIdAsync(branchId);

            return Ok(result);
        }

        // ============================================================
        // Add Branch
        // ============================================================

        [HttpPost]
        public async Task<ActionResult<BranchResponseDto>> AddBranch(
            [FromBody] AddBranchRequestDto request)
        {
            var result = await _branchManagementService.AddBranchAsync(request);

            return CreatedAtAction(
                nameof(GetBranchById),
                new { branchId = result.Id },
                result
            );
        }

        // ============================================================
        // Update Branch
        // ============================================================

        [HttpPut("{branchId:int}")]
        public async Task<ActionResult<BranchResponseDto>> UpdateBranch(
            [FromRoute] int branchId,
            [FromBody] UpdateBranchRequestDto request)
        {
            var result = await _branchManagementService.UpdateBranchAsync(branchId, request);

            return Ok(result);
        }

        // ============================================================
        // Soft Delete Branch
        // ============================================================

        [HttpDelete("{branchId:int}")]
        public async Task<IActionResult> SoftDeleteBranch(
            [FromRoute] int branchId)
        {
            await _branchManagementService.SoftDeleteBranchAsync(branchId);

            return NoContent();
        }

        // ============================================================
        // Activate Branch
        // ============================================================

        [HttpPatch("{branchId:int}/activate")]
        public async Task<IActionResult> ActivateBranch(
            [FromRoute] int branchId)
        {
            await _branchManagementService.ActivateBranchAsync(branchId);

            return NoContent();
        }

        // ============================================================
        // Deactivate Branch
        // ============================================================

        [HttpPatch("{branchId:int}/deactivate")]
        public async Task<IActionResult> DeactivateBranch(
            [FromRoute] int branchId)
        {
            await _branchManagementService.DeactivateBranchAsync(branchId);

            return NoContent();
        }
    }
}