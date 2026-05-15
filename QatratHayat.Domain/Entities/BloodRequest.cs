using QatratHayat.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace QatratHayat.Domain.Entities
{
    public class BloodRequest
    {
        public int Id { get; set; }

        [Required]
        public RelationshipType RelationshipType { get; set; }

        public BloodType? BloodType { get; set; }

        public int? UnitsNeeded { get; set; }

        public UrgencyLevel? UrgencyLevel { get; set; }

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

        public DateTime? PublishedAt { get; set; }

        public int? PublishedByUserId { get; set; }

        [MaxLength(500)]
        public string? RejectionReason { get; set; }

        public DateTime? RejectedAt { get; set; }

        public int? RejectedByUserId { get; set; }

        public DateTime? UpdatedAt { get; set; }

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
        public int BranchId { get; set; }

        public Branch Branch { get; set; } = null!;

        [Required]
        public int RequesterUserId { get; set; }

        [Required]
        public int DoctorId { get; set; }

        public int? CancelledByUserId { get; set; }
    }
}