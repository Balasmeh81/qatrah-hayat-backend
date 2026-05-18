using QatratHayat.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace QatratHayat.Application.Features.BloodRequests.DTOS
{
    public class CreateBloodRequestDto
    {
        [Required]
        public RelationshipType RelationshipType { get; set; }

        [Required]
        public int HospitalId { get; set; }

        [Required]
        public int DoctorId { get; set; }
        [Required]
        public BloodType BloodType { get; set; }

        [StringLength(10, MinimumLength = 10)]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "National ID must be exactly 10 digits.")]
        public string? BeneficiaryNationalId { get; set; }
    }
}