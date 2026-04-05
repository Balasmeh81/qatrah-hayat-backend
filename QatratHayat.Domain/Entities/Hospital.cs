using System.ComponentModel.DataAnnotations;

namespace QatratHayat.Domain.Entities
{
    public class Hospital
    {
        public int Id { get; set; }
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
        public bool IsActive { get; set; }
        [Required]
        public bool IsDeleted { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; } 

        public DateTime? UpdatedAt { get; set; }

        // Navigation Property
        [Required]
        public int BranchId { get; set; }
        public Branch Branch { get; set; }= null!;

        public ICollection<BloodRequest> BloodRequests { get; set; } = new List<BloodRequest>();
    }
}