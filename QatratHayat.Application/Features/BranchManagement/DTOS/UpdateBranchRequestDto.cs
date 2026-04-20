using System.ComponentModel.DataAnnotations;

namespace QatratHayat.Application.Features.BranchManagement.DTOS
{
    public class UpdateBranchRequestDto
    {
        [Required]
        [MaxLength(256)]
        public string BranchNameAr { get; set; } = null!;

        [Required]
        [MaxLength(256)]
        public string BranchNameEn { get; set; } = null!;

        [Required]
        [MaxLength(500)]
        public string AddressAr { get; set; } = null!;

        [Required]
        [MaxLength(500)]
        public string AddressEn { get; set; } = null!;

        [Required]
        public int ManagerUserId { get; set; }

        public bool IsActive { get; set; }

        [Range(-90, 90)]
        public decimal GPSLat { get; set; }

        [Range(-180, 180)]
        public decimal GPSLng { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        [RegularExpression(@"^07\d{8}$", ErrorMessage = "Phone number must start with 07 and contain exactly 10 digits.")]
        public string? Phone { get; set; }
    }
}