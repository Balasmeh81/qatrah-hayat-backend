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

        [MaxLength(10)]
        [RegularExpression(@"^0\d{0,9}$", ErrorMessage = "Phone number must start with 0 and contain digits only, with a maximum length of 10 digits.")]
        public string? Phone { get; set; }

        [Required]
        [MinLength(7, ErrorMessage = "Working hours must contain all 7 days.")]
        [MaxLength(7, ErrorMessage = "Working hours must contain all 7 days.")]
        public List<BranchWorkingHourRequestDto> WorkingHours { get; set; } = new();
    }
}