using QatratHayat.Domain.Enums;
using System.ComponentModel.DataAnnotations;
namespace QatratHayat.Application.Accounts.DTOs
{
    //This Class represents data coming from user SignUp.
    public class RegisterRequestDto
    {
        [Required]
        [StringLength(10, MinimumLength = 10)]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "National ID must be exactly 10 digits.")]
        public string NationalId { get; set; } = null!;
        [Required]
        public string FullNameAr { get; set; } = null!;
        [Required]
        public string FullNameEn { get; set; } = null!;
        [Required]
        public DateTime DateOfBirth { get; set; }
        [Required]
        public BloodType BloodType { get; set; }
        [Required]
        public Gender Gender { get; set; }
        [Required]
        public MaritalStatus MaritalStatus { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
        [Required]
        [Phone]
        [RegularExpression(@"^(\+962|0)?7[789]\d{7}$", ErrorMessage = "Invalid Jordanian phone number.")]
        public string PhoneNumber { get; set; } = null!;
        public string? JobTitle { get; set; }
        public string? Address { get; set; }
        [Required]
        [MinLength(8)]
        public string Password { get; set; } = null!;
        [Required]
        [Compare(nameof(Password))]
        public string ConfirmPassword { get; set; } = null!;
    }
}
