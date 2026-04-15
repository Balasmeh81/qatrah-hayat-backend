

using QatratHayat.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace QatratHayat.Domain.Entities
{
    public class Donation
    {
        public int Id { get; set; }
        [Required]
        public DonationType DonationType { get; set; }
        [Required]
        public InitialEligibilityStatus InitialEligibilityStatus { get; set; }
        [Required]
        public FinalEligibilityStatus FinalEligibilityStatus { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }

        [MaxLength(500)]
        public string? FinalDecisionReason { get; set; }

        // Navigation Property
        [Required]
        public int DonorProfileId { get; set; }
        public DonorProfile DonorProfile { get; set; } = null!;

        [Required]
        public int EmployeeUserId { get; set; }

        [Required]
        public int BranchId { get; set; }
        public Branch Branch { get; set; } = null!;

        public int? BloodRequestId { get; set; }
        public BloodRequest? BloodRequest { get; set; }

        public BloodUnit BloodUnit { get; set; } = null!;

        public int? CampaignId { get; set; }
        public Campaign? Campaign { get; set; }


    }
}
