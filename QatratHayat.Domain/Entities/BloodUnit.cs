using QatratHayat.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace QatratHayat.Domain.Entities
{
    public class BloodUnit
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(20)]
        public string UnitCode { get; set; } = null!;
        [Required]
        public BloodType BloodType { get; set; }
        [Required]
        public DateTime CollectionDate { get; set; }
        [Required]
        public DateTime ExpiresAt { get; set; }
        [Required]
        public UnitStatus UnitStatus { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }

        public DateTime? AllocatedAt { get; set; }
        public DateTime? DisposalDate { get; set; }
        public DateTime? UpdatedAt { get; set; }
        [MaxLength(500)]
        public string? DisposalReason { get; set; }
        [MaxLength(500)]
        public string? DeallocationNote { get; set; }

        // Navigation Property
        public int? AllocatedToRequestId { get; set; }
        public BloodRequest? BloodRequest { get; set; }

        [Required]
        public int BranchId { get; set; }
        public Branch Branch { get; set; } = null!;

        [Required]
        public int DonationId { get; set; }
        public Donation Donation { get; set; } = null!;

        public int? DisposedByEmployeeId { get; set; }
    }
}
