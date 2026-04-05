using QatratHayat.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace QatratHayat.Domain.Entities
{
    public class BloodRequest
    {
        public int Id { get; set; }
        [Required]
        public RelationshipType RelationshipType { get; set; }
        [Required]
        public BloodType BloodType { get; set; }
        [Required]
        public int UnitsNeeded { get; set; }
        [Required]
        public UrgencyLevel UrgencyLevel { get; set; }
        [Required]
        public RequestStatus RequestStatus { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }

        [MaxLength(500)]
        public string? ClinicalNotes { get; set; }
        public DateTime? DoctorApprovedAt { get; set; }
        [MaxLength(500)]
        public string? CancellationReason { get; set; }
        public DateTime? CancelledAt { get; set; }
        public DateTime? ShortagePhase1At { get; set; }
        public DateTime? ShortagePhase2At { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation Property
        public ICollection<DonationIntent> DonationIntents { get; set; } = new List<DonationIntent>();
        public ICollection<Donation> Donations { get; set; } = new List<Donation>();
        public ICollection<BloodUnit> BloodUnits { get; set; } = new List<BloodUnit>();

        [Required]
        public int BeneficiaryId { get; set; }
        public Beneficiary Beneficiary { get; set; } = null!;

        [Required]
        public int HospitalId { get; set; }
        public Hospital Hospital { get; set; } = null!;

        [Required]
        public int RequesterUserId { get; set; }

        [Required]
        public int DoctorId { get; set; }

        public int? CancelledByUserId { get; set; }
    }
}
