using Microsoft.AspNetCore.Mvc;
using QatratHayat.Application.Features.Accounts.DTOs;
using QatratHayat.Application.Features.Auth.Interfaces;

namespace QatratHayat.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CivilStatusController : ControllerBase
    {
        private readonly ICivilStatusService civilStatusService;

        public CivilStatusController(ICivilStatusService _civilStatusService)
        {
            civilStatusService = _civilStatusService;
        }

        [HttpGet("{nationalId}")]
        public async Task<ActionResult<NationalRegistryResponseDto>> GetByNationalId(string nationalId)
        {
            var result = await civilStatusService.GetNationalRegistryAsync(nationalId);
            return Ok(result);
        }
    }
}
