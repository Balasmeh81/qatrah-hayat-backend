using Microsoft.AspNetCore.Mvc;
using QatratHayat.Application.Accounts.DTOs;
using QatratHayat.Application.Accounts.Services;

namespace QatratHayat.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAccountService accountService;

        // Constructor
        public AuthController(IAccountService _accountService)
        {
            accountService = _accountService;
        }

        // Register new citizen account
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequestDto request)
        {
            var result = await accountService.RegisterAsync(request);
            return Ok(result);
        }

        // Login using email or national ID
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestDto request)
        {
            var result = await accountService.LoginAsync(request);
            return Ok(result);
        }
    }
}