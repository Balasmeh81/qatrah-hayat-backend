using Microsoft.AspNetCore.Mvc;
using QatratHayat.Application.Common.DTOS;
using QatratHayat.Application.Features.HospitalManagement.DOTS;
using QatratHayat.Application.Features.HospitalManagement.DTOS;
using QatratHayat.Application.Features.HospitalManagement.Interfaces;

namespace QatratHayat.API.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/hospitals")]
    //[Authorize(Roles = "Admin")]
    public class HospitalManagementController : ControllerBase
    {
        private readonly IHospitalManagementService _hospitalManagementService;

        public HospitalManagementController(
            IHospitalManagementService hospitalManagementService)
        {
            _hospitalManagementService = hospitalManagementService;
        }

        // ============================================================
        // Get All Hospitals
        // ============================================================

        [HttpGet]
        public async Task<ActionResult<PagedResultDto<HospitalResponseDto>>> GetAllHospitals(
            [FromQuery] HospitalQueryDto query)
        {
            var result = await _hospitalManagementService.GetAllHospitalsAsync(query);

            return Ok(result);
        }

        // ============================================================
        // Get Hospital By Id
        // ============================================================

        [HttpGet("{hospitalId:int}")]
        public async Task<ActionResult<HospitalResponseDto>> GetHospitalById(
            [FromRoute] int hospitalId)
        {
            var result = await _hospitalManagementService.GetHospitalByIdAsync(hospitalId);

            return Ok(result);
        }

        // ============================================================
        // Add Hospital
        // ============================================================

        [HttpPost]
        public async Task<ActionResult<HospitalResponseDto>> AddHospital(
            [FromBody] AddHospitalRequestDto request)
        {
            var result = await _hospitalManagementService.AddHospitalAsync(request);

            return CreatedAtAction(
                nameof(GetHospitalById),
                new { hospitalId = result.Id },
                result
            );
        }

        // ============================================================
        // Update Hospital
        // ============================================================

        [HttpPut("{hospitalId:int}")]
        public async Task<ActionResult<HospitalResponseDto>> UpdateHospital(
            [FromRoute] int hospitalId,
            [FromBody] UpdateHospitalRequestDto request)
        {
            var result = await _hospitalManagementService.UpdateHospitalAsync(
                hospitalId,
                request
            );

            return Ok(result);
        }

        // ============================================================
        // Soft Delete Hospital
        // ============================================================

        [HttpDelete("{hospitalId:int}")]
        public async Task<IActionResult> SoftDeleteHospital(
            [FromRoute] int hospitalId)
        {
            await _hospitalManagementService.SoftDeleteHospitalAsync(hospitalId);

            return NoContent();
        }

        // ============================================================
        // Activate Hospital
        // ============================================================

        [HttpPatch("{hospitalId:int}/activate")]
        public async Task<IActionResult> ActivateHospital(
            [FromRoute] int hospitalId)
        {
            await _hospitalManagementService.ActivateHospitalAsync(hospitalId);

            return NoContent();
        }

        // ============================================================
        // Deactivate Hospital
        // ============================================================

        [HttpPatch("{hospitalId:int}/deactivate")]
        public async Task<IActionResult> DeactivateHospital(
            [FromRoute] int hospitalId)
        {
            await _hospitalManagementService.DeactivateHospitalAsync(hospitalId);

            return NoContent();
        }
    }
}