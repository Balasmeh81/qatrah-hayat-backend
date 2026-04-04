using System.ComponentModel.DataAnnotations;

namespace QatratHayat.Application.Accounts.DTOs
{
    public class LoginRequestDto
    {
        [Required]
        public string EmailOrNationalId { get; set; } = null!;
        [Required]
        public string Password { get; set; } = null!;
    }
}