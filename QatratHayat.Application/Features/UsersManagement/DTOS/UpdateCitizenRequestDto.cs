using QatratHayat.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace QatratHayat.Application.Features.UsersManagement.DTOS
{
    public class UpdateCitizenRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [RegularExpression(@"^07\d{8}$", ErrorMessage = "Phone number must start with 07 and contain exactly 10 digits.")]
        public string PhoneNumber { get; set; } = null!;
        public BloodTypeStatus BloodTypeStatus { get; set; }
        public EligibilityStatus EligibilityStatus { get; set; }
        public string? PermanentDeferralReason { get; set; }
        public bool IsActive { get; set; }
    }
}