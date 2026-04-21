using System.ComponentModel.DataAnnotations;

namespace QatratHayat.Application.Features.Auth.DTOs
{
    public class ForgotPasswordRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
    }
}