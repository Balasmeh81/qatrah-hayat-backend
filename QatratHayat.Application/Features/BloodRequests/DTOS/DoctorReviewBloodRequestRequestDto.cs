using QatratHayat.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace QatratHayat.Application.Features.BloodRequests.DTOS
{
    public class DoctorReviewBloodRequestRequestDto
    {
        [Required]
        public bool IsApproved { get; set; }

        public BloodType? BloodType { get; set; }

        public int? UnitsNeeded { get; set; }

        public UrgencyLevel? UrgencyLevel { get; set; }

        [MaxLength(500)]
        public string? ClinicalNotes { get; set; }

        [MaxLength(500)]
        public string? RejectionReason { get; set; }
    }
}