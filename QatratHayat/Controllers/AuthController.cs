using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QatratHayat.Application.Accounts.DTOs;
using QatratHayat.Application.Common.Interfaces;

namespace QatratHayat.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        // Service that contains authentication business logic.
        private readonly IAccountService accountService;

        public AuthController(IAccountService _accountService)
        {
            accountService = _accountService;
        }
        // POST: api/Auth/register
        // Creates a new citizen account, then returns user data + JWT token.
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register(RegisterRequestDto request)
        {
            var result = await accountService.RegisterAsync(request);
            return Ok(result);
        }
        // POST: api/Auth/login
        // Logs in the user using email or national ID, then returns user data + JWT token.
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginRequestDto request)
        {
            var result = await accountService.LoginAsync(request);
            return Ok(result);
        }
        // POST: api/Auth/login
        // Logs in the user using email or national ID, then returns user data + JWT token.
        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<CurrentUserDto>> GetCurrentUser()
        {
            var result = await accountService.GetCurrentUserAsync(User);
            return Ok(result);
        }
    }
}