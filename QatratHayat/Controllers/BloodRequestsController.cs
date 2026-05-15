using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QatratHayat.Application.Common.DTOS;
using QatratHayat.Application.Features.BloodRequests.DTOS;
using QatratHayat.Application.Features.BloodRequests.Interfaces;

namespace QatratHayat.API.Controllers.BloodRequestsControllers
{
    [ApiController]
    [Route("api/blood-requests")]
    [Authorize]
    public class BloodRequestsController : ControllerBase
    {
        private readonly IBloodRequestService _bloodRequestService;

        public BloodRequestsController(IBloodRequestService bloodRequestService)
        {
            _bloodRequestService = bloodRequestService;
        }

        // ============================================================
        // Citizen Endpoints
        // ============================================================
        [HttpGet("citizen-data")]
        [Authorize(Roles = "Citizen")]
        public async Task<ActionResult<CitizenDataResponseDto>> GetCurrentCitizenData()
        {
            var result = await _bloodRequestService.GetCurrentCitizenDataAsync();

            return Ok(result);
        }

        [HttpGet("beneficiary-lookup/{nationalId}")]
        [Authorize(Roles = "Citizen")]
        public async Task<ActionResult<CitizenDataResponseDto>> LookupBeneficiaryByNationalId(
            [FromRoute] string nationalId
        )
        {
            var result = await _bloodRequestService.LookupBeneficiaryByNationalIdAsync(
                nationalId
            );

            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Citizen")]
        public async Task<ActionResult<BloodRequestDetailsResponseDto>> CreateBloodRequest(
            [FromBody] CreateBloodRequestDto dto
        )
        {
            var result = await _bloodRequestService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetBloodRequestById),
                new { requestId = result.Id },
                result
            );
        }

        [HttpGet("my")]
        [Authorize(Roles = "Citizen")]
        public async Task<ActionResult<PagedResultDto<BloodRequestResponseDto>>> GetMyRequests(
            [FromQuery] BloodRequestQueryDto query
        )
        {
            var result = await _bloodRequestService.GetMyRequestsAsync(query);

            return Ok(result);
        }

        // ============================================================
        // Shared Endpoint
        // ============================================================

        [HttpGet("{requestId:int}")]
        [Authorize(Roles = "Citizen,Doctor,Employee,BranchManager,Admin")]
        public async Task<ActionResult<BloodRequestDetailsResponseDto>> GetBloodRequestById(
            [FromRoute] int requestId
        )
        {
            var result = await _bloodRequestService.GetByIdAsync(requestId);

            return Ok(result);
        }

        [HttpPatch("{requestId:int}/cancel")]
        [Authorize(Roles = "Citizen,Doctor,Employee,BranchManager,Admin")]
        public async Task<ActionResult<BloodRequestDetailsResponseDto>> CancelBloodRequest(
            [FromRoute] int requestId,
            [FromBody] CancelBloodRequestDto dto
        )
        {
            var result = await _bloodRequestService.CancelAsync(requestId, dto);

            return Ok(result);
        }

        // ============================================================
        // Doctor Endpoints
        // ============================================================

        [HttpGet("doctor")]
        [Authorize(Roles = "Doctor")]
        public async Task<ActionResult<PagedResultDto<BloodRequestResponseDto>>> GetDoctorRequests(
            [FromQuery] BloodRequestQueryDto query
        )
        {
            var result = await _bloodRequestService.GetDoctorRequestsAsync(query);

            return Ok(result);
        }

        [HttpPatch("{requestId:int}/doctor-review")]
        [Authorize(Roles = "Doctor")]
        public async Task<ActionResult<BloodRequestDetailsResponseDto>> DoctorReviewBloodRequest(
            [FromRoute] int requestId,
            [FromBody] DoctorReviewBloodRequestRequestDto dto
        )
        {
            var result = await _bloodRequestService.DoctorReviewAsync(requestId, dto);

            return Ok(result);
        }

        // ============================================================
        // Employee / Branch Manager Endpoints
        // ============================================================

        [HttpGet("branch")]
        [Authorize(Roles = "Employee,BranchManager")]
        public async Task<ActionResult<PagedResultDto<BloodRequestResponseDto>>> GetBranchRequests(
            [FromQuery] BloodRequestQueryDto query
        )
        {
            var result = await _bloodRequestService.GetBranchRequestsAsync(query);

            return Ok(result);
        }

        [HttpPatch("{requestId:int}/employee-review")]
        [Authorize(Roles = "Employee,BranchManager")]
        public async Task<ActionResult<BloodRequestDetailsResponseDto>> EmployeeReviewBloodRequest(
            [FromRoute] int requestId,
            [FromBody] EmployeeReviewBloodRequestRequestDto dto
        )
        {
            var result = await _bloodRequestService.EmployeeReviewAsync(requestId, dto);

            return Ok(result);
        }

        [HttpPatch("{requestId:int}/confirm-allocation")]
        [Authorize(Roles = "Employee,BranchManager")]
        public async Task<ActionResult<BloodRequestDetailsResponseDto>> ConfirmBloodRequestAllocation(
            [FromRoute] int requestId,
            [FromBody] ConfirmBloodRequestAllocationRequestDto dto
        )
        {
            var result = await _bloodRequestService.ConfirmAllocationAsync(requestId, dto);

            return Ok(result);
        }

        [HttpPatch("{requestId:int}/publish")]
        [Authorize(Roles = "Employee,BranchManager")]
        public async Task<ActionResult<BloodRequestDetailsResponseDto>> PublishBloodRequest(
            [FromRoute] int requestId
        )
        {
            var result = await _bloodRequestService.PublishAsync(requestId);

            return Ok(result);
        }

        [HttpPatch("{requestId:int}/reject")]
        [Authorize(Roles = "Employee,BranchManager")]
        public async Task<ActionResult<BloodRequestDetailsResponseDto>> RejectBloodRequest(
            [FromRoute] int requestId,
            [FromBody] RejectBloodRequestRequestDto dto
        )
        {
            var result = await _bloodRequestService.RejectAsync(requestId, dto);

            return Ok(result);
        }
    }
}