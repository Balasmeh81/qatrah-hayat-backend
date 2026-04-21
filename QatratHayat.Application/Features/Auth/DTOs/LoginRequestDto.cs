using System.ComponentModel.DataAnnotations;

namespace QatratHayat.Application.Features.Accounts.DTOs
{
    public class LoginRequestDto
    {
        [Required]
        public string NationalId { get; set; } = null!;
        [Required]
        public string Password { get; set; } = null!;

        public bool RememberMe { get; set; }
    }
}