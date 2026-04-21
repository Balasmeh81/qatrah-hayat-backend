using System.ComponentModel.DataAnnotations;

namespace QatratHayat.Application.Features.Auth.DTOs
{
    public class VerifyResetOtpRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [StringLength(6, MinimumLength = 6)]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "OTP must be exactly 6 digits.")]
        public string Otp { get; set; } = null!;
    }
}