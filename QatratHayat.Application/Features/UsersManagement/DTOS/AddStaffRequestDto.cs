using QatratHayat.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace QatratHayat.Application.Features.UsersManagement.DTOS
{
    public class AddStaffRequestDto
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
        [RegularExpression(@"^07\d{8}$", ErrorMessage = "Phone number must start with 07 and contain exactly 10 digits.")]
        public string PhoneNumber { get; set; } = null!;
        public string? Password { get; set; }
        public string? ConfirmPassword { get; set; }
        [Required]
        public UserRole StaffRole { get; set; }
        public int? BranchId { get; set; }
        public int? HospitalId { get; set; }

    }
}
