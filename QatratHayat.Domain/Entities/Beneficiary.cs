using System.ComponentModel.DataAnnotations;

namespace QatratHayat.Domain.Entities
{
    public class Beneficiary
    {

        public int Id { get; set; }
        [Required]
        [StringLength(10, MinimumLength = 10)]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "National ID must be exactly 10 digits.")]
        public string NationalId { get; set; } = null!;
        [Required]
        [MaxLength(256)]
        public string FullNameAr { get; set; } = null!;
        [Required]
        [MaxLength(256)]
        public string FullNameEn { get; set; } = null!;
        [Required]
        public bool IsTemporary { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }

        public DateTime? MergedAt { get; set; }

        // Navigation Property
        public int? UserId { get; set; }
        public int? MergedIntoUserId { get; set; }

        public ICollection<BloodRequest> BloodRequests { get; set; } = new List<BloodRequest>();
    }
}