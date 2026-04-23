using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QatratHayat.Application.Common.Interfaces;
using QatratHayat.Application.Features.Accounts.DTOs;
using QatratHayat.Application.Features.Auth.DTOs;
using QatratHayat.Application.Features.Auth.Interfaces;

namespace QatratHayat.API.Controllers.AuthControllers
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
        [HttpPost("registerCitizen")]
        public async Task<ActionResult<RegisterResponseDto>> RegisterCitizenAsync(
            RegisterRequestDto request
        )
        {
            var result = await accountService.RegisterCitizenAsync(request);
            return Ok(result);
        }

        // POST: api/Auth/login
        // Logs in the user using email or national ID, then returns user data + JWT token.
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login(LoginRequestDto request)
        {
            var result = await accountService.LoginAsync(request);
            return Ok(result);
        }

        // POST: api/Auth/login
        // Logs in the user using  national ID, then returns user data + JWT token.
        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<CurrentUserDto>> GetCurrentUser()
        {
            var result = await accountService.GetCurrentUserAsync(User);
            return Ok(result);
        }

        [HttpPost("forgot-password")]
        public async Task<ActionResult<ForgotPasswordMessageResponseDto>> ForgotPassword(
            ForgotPasswordRequestDto request
        )
        {
            var result = await accountService.ForgotPasswordAsync(request);
            return Ok(result);
        }

        [HttpPost("verify-reset-otp")]
        public async Task<ActionResult<VerifyResetOtpResponseDto>> VerifyResetOtp(
            VerifyResetOtpRequestDto request
        )
        {
            var result = await accountService.VerifyResetOtpAsync(request);
            return Ok(result);
        }

        [HttpPost("reset-password")]
        public async Task<ActionResult<ForgotPasswordMessageResponseDto>> ResetPassword(
            ResetPasswordRequestDto request
        )
        {
            var result = await accountService.ResetPasswordAsync(request);
            return Ok(result);
        }

        [HttpPost("test-email")]
        public async Task<IActionResult> TestEmail(
            [FromServices] IEmailService emailService,
            [FromBody] string email
        )
        {
            await emailService.SendPasswordResetOtpAsync(email, "Test User", "123456", false);

            return Ok(new { message = "Test email sent successfully." });
        }
    }
}
