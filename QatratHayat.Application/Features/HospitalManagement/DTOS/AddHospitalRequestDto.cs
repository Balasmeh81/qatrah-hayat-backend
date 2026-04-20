using System.ComponentModel.DataAnnotations;

namespace QatratHayat.Application.Features.HospitalManagement.DTOS
{
    public class AddHospitalRequestDto
    {
        [Required]
        [MaxLength(256)]
        public string HospitalNameAr { get; set; } = null!;

        [Required]
        [MaxLength(256)]
        public string HospitalNameEn { get; set; } = null!;

        [Required]
        [MaxLength(500)]
        public string AddressAR { get; set; } = null!;

        [Required]
        [MaxLength(500)]
        public string AddressEn { get; set; } = null!;

        [Required]
        public int BranchId { get; set; }
    }
}