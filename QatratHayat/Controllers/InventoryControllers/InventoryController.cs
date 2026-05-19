using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QatratHayat.Application.Common.DTOS;
using QatratHayat.Application.Features.Inventory.DTOs;
using QatratHayat.Application.Features.Inventory.Interfaces;
using System.Security.Claims;

namespace QatratHayat.API.Controllers
{
    [ApiController]
    [Route("api/inventory")]
    [Authorize]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;

        public InventoryController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        [HttpGet("blood-units")]
        [Authorize(Roles = "Employee,BranchManager,Admin")]
        public async Task<ActionResult<PagedResultDto<BloodUnitListItemDto>>> GetBloodUnits(
            [FromQuery] BloodUnitQueryDto query
        )
        {
            var userId = GetCurrentUserId();

            var result = await _inventoryService.GetBloodUnitsAsync(
                userId,
                query
            );

            return Ok(result);
        }

        [HttpGet("blood-units/{id:int}")]
        [Authorize(Roles = "Employee,BranchManager,Admin")]
        public async Task<ActionResult<BloodUnitDetailsDto>> GetBloodUnitById(
            int id
        )
        {
            var userId = GetCurrentUserId();

            var result = await _inventoryService.GetBloodUnitByIdAsync(
                userId,
                id
            );

            return Ok(result);
        }

        [HttpGet("statistics")]
        [Authorize(Roles = "Employee,BranchManager,Admin")]
        public async Task<ActionResult<InventoryStatisticsDto>> GetStatistics(
            [FromQuery] int? branchId
        )
        {
            var userId = GetCurrentUserId();

            var result = await _inventoryService.GetStatisticsAsync(
                userId,
                branchId
            );

            return Ok(result);
        }

        [HttpPatch("blood-units/mark-expired")]
        [Authorize(Roles = "Employee,BranchManager,Admin")]
        public async Task<ActionResult<object>> MarkExpiredUnits()
        {
            var userId = GetCurrentUserId();

            var updatedCount = await _inventoryService.MarkExpiredUnitsAsync(
                userId
            );

            return Ok(new
            {
                UpdatedCount = updatedCount,
                Message = "Expired blood units were updated successfully."
            });
        }

        [HttpPatch("blood-units/{id:int}/dispose")]
        [Authorize(Roles = "Employee,BranchManager")]
        public async Task<ActionResult<BloodUnitDetailsDto>> DisposeBloodUnit(
            int id,
            [FromBody] DisposeBloodUnitRequestDto request
        )
        {
            var userId = GetCurrentUserId();

            var result = await _inventoryService.DisposeBloodUnitAsync(
                userId,
                id,
                request
            );

            return Ok(result);
        }

        [HttpPatch("blood-units/{id:int}/return-to-available")]
        [Authorize(Roles = "Employee,BranchManager")]
        public async Task<ActionResult<BloodUnitDetailsDto>> ReturnBloodUnitToAvailable(
            int id,
            [FromBody] ReturnBloodUnitToAvailableRequestDto request
        )
        {
            var userId = GetCurrentUserId();

            var result = await _inventoryService.ReturnBloodUnitToAvailableAsync(
                userId,
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