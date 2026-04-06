using QatratHayat.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace QatratHayat.Domain.Entities
{
    public class DonorProfile
    {
        public int Id { get; set; }
        [Required]
        public BloodType BloodType { get; set; }
        [Required]
        public BloodTypeStatus BloodTypeStatus { get; set; }
        [Required]
        public EligibilityStatus EligibilityStatus { get; set; }
        [Required]
        public int DonationCount { get; set; }
        [Required]
        public bool iAgree { get; set; }
        [Required]
        public bool iConfirm { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }

        [MaxLength(500)]
        public string? PermanentDeferralReason { get; set; }
        public DateTime? LastDonationDate { get; set; }
        public DateTime? NextEligibleDate { get; set; }
        public DateTime? BloodTypeConfirmedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }


        // Navigation Property
        [Required]
        public int UserId { get; set; }
        public int? BloodTypeConfirmedByEmployeeId { get; set; }

        public ICollection<DeferralRecord> DeferralRecords { get; set; } = new List<DeferralRecord>();
        public ICollection<ScreeningAnswer> ScreeningAnswers { get; set; } = new List<ScreeningAnswer>();
        public ICollection<Donation> Donations { get; set; } = new List<Donation>();
        public ICollection<DonationIntent> DonationIntents { get; set; } = new List<DonationIntent>();
    }
}