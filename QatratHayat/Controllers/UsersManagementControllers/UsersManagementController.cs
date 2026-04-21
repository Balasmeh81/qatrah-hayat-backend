using Microsoft.AspNetCore.Mvc;
using QatratHayat.Application.Common.DTOS;
using QatratHayat.Application.Features.UsersManagement.DTOS;
using QatratHayat.Application.Features.UsersManagement.Interfaces;

namespace QatratHayat.API.Controllers.UsersManagementControllers
{
    [ApiController]
    [Route("api/users-management")]
    //[Authorize(Roles = "Admin")]
    public class UsersManagementController : ControllerBase
    {
        private readonly IUsersManagementService _usersManagementService;

        public UsersManagementController(IUsersManagementService usersManagementService)
        {
            _usersManagementService = usersManagementService;
        }

        // ============================================================
        // Staff Endpoints
        // ============================================================

        [HttpGet("staff")]
        public async Task<ActionResult<PagedResultDto<StaffInfoResponseDto>>> GetAllStaffUsers(
            [FromQuery] UserManagementQueryDto query
        )
        {
            var result = await _usersManagementService.GetAllStaffUsersAsync(query);

            return Ok(result);
        }

        [HttpGet("staff/{userId:int}")]
        public async Task<ActionResult<StaffInfoResponseDto>> GetStaffById(
            [FromRoute] int userId
        )
        {
            var result = await _usersManagementService.GetStaffByIdAsync(userId);

            return Ok(result);
        }

        [HttpPost("staff/create-from-national-registry")]
        public async Task<ActionResult<StaffInfoResponseDto>> CreateStaffFromNationalRegistry(
            [FromBody] CreateStaffFromRegistryRequestDto dto
        )
        {
            var result = await _usersManagementService.CreateStaffFromNationalRegistryAsync(dto);

            return CreatedAtAction(
                nameof(GetStaffById),
                new { userId = result.UserId },
                result
            );
        }

        [HttpPut("staff/{userId:int}")]
        public async Task<ActionResult<StaffInfoResponseDto>> UpdateStaff(
            [FromRoute] int userId,
            [FromBody] UpdateStaffRequestDto dto
        )
        {
            var result = await _usersManagementService.UpdateStaffAsync(userId, dto);

            return Ok(result);
        }

        // ============================================================
        // Citizen Endpoints
        // ============================================================

        [HttpGet("citizens")]
        public async Task<ActionResult<PagedResultDto<CitizenInfoResponseDto>>> GetAllCitizenUsers(
            [FromQuery] UserManagementQueryDto query
        )
        {
            var result = await _usersManagementService.GetAllCitizenUsersAsync(query);

            return Ok(result);
        }

        [HttpGet("citizens/lookup/{nationalId}")]
        public async Task<ActionResult<CitizenResponseDto>> LookupCitizenByNationalId(
            [FromRoute] string nationalId
        )
        {
            var result = await _usersManagementService.LookupCitizenByNationalIdAsync(nationalId);

            return Ok(result);
        }

        [HttpGet("citizens/{userId:int}")]
        public async Task<ActionResult<CitizenInfoResponseDto>> GetCitizenById(
            [FromRoute] int userId
        )
        {
            var result = await _usersManagementService.GetCitizenByIdAsync(userId);

            return Ok(result);
        }

        [HttpPost("citizens/{userId:int}/promote-to-staff")]
        public async Task<ActionResult<StaffInfoResponseDto>> PromoteCitizenToStaff(
            [FromRoute] int userId,
            [FromBody] PromoteCitizenToStaffRequestDto dto
        )
        {
            var result = await _usersManagementService.PromoteCitizenToStaffAsync(userId, dto);

            return Ok(result);
        }

        [HttpPut("citizens/{userId:int}")]
        public async Task<ActionResult<CitizenInfoResponseDto>> UpdateCitizen(
            [FromRoute] int userId,
            [FromBody] UpdateCitizenRequestDto dto
        )
        {
            var result = await _usersManagementService.UpdateCitizenAsync(userId, dto);

            return Ok(result);
        }

        // ============================================================
        // Shared User Actions
        // ============================================================

        [HttpPatch("{userId:int}/activate")]
        public async Task<IActionResult> ActivateUser(
            [FromRoute] int userId
        )
        {
            await _usersManagementService.ActivateUserAsync(userId);

            return NoContent();
        }

        [HttpPatch("{userId:int}/deactivate")]
        public async Task<IActionResult> DeactivateUser(
            [FromRoute] int userId
        )
        {
            await _usersManagementService.DeactivateUserAsync(userId);

            return NoContent();
        }

        [HttpDelete("{userId:int}")]
        public async Task<IActionResult> SoftDeleteUser(
            [FromRoute] int userId
        )
        {
            await _usersManagementService.SoftDeleteUserAsync(userId);

            return NoContent();
        }

        // ============================================================
        // Statistics
        // ============================================================

        [HttpGet("statistics")]
        public async Task<ActionResult<UsersStatisticsResponseDto>> GetStatistics()
        {
            var result = await _usersManagementService.GetStatisticsAsync();

            return Ok(result);
        }
    }
}