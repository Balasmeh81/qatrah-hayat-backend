using QatratHayat.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace QatratHayat.Application.Features.Donations.DTOs
{
    public class FinalAssessmentRequestDto
    {
        [Required]
        public FinalEligibilityStatus FinalEligibilityStatus { get; set; }

        [MaxLength(500)]
        public string? FinalDecisionReason { get; set; }

        public BloodType? ConfirmedBloodType { get; set; }

        public bool ConfirmBloodType { get; set; }

        public DateTime? TemporaryDeferralEndDate { get; set; }
    }
}