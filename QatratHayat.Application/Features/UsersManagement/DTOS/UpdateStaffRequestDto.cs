using QatratHayat.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace QatratHayat.Application.Features.UsersManagement.DTOS
{
    public class UpdateStaffRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [RegularExpression(@"^07\d{8}$", ErrorMessage = "Phone number must start with 07 and contain exactly 10 digits.")]
        public string PhoneNumber { get; set; } = null!;

        public UserRole StaffRole { get; set; }

        public int? BranchId { get; set; }

        public int? HospitalId { get; set; }

        public bool IsActive { get; set; }
    }
}