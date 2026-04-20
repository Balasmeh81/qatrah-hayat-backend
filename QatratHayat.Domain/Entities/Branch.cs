using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QatratHayat.Domain.Entities
{
    public class Branch
    {
        public int Id { get; set; }
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
        public bool IsActive { get; set; }
        [Required]
        public bool IsDeleted { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }
        [Required]
        [Column(TypeName = "decimal(9,6)")]
        public decimal GPSLat { get; set; }
        [Required]
        [Column(TypeName = "decimal(9,6)")]
        public decimal GPSLng { get; set; }

        [EmailAddress]
        public string? Email { get; set; }
        [Phone]
        public string? Phone { get; set; }

        public DateTime? UpdatedAt { get; set; }


        // Navigation Property

        [Required]
        public int ManagerUserId { get; set; }

        public ICollection<Hospital> Hospitals { get; set; } = new List<Hospital>();
        public ICollection<BranchWorkingHour> WorkingHours { get; set; } = new List<BranchWorkingHour>();
        public ICollection<Campaign> Campaigns { get; set; } = new List<Campaign>();
        public ICollection<BloodUnit> BloodUnits { get; set; } = new List<BloodUnit>();
        public ICollection<Donation> Donations { get; set; } = new List<Donation>();
    }
}