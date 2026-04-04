using System.ComponentModel.DataAnnotations;

namespace QatratHayat.Domain.Entities
{
    public class Beneficiary
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(20)]
        public string NationalId { get; set; } = null!;

        [Required]
        [MaxLength(200)]
        public string FullName { get; set; } = null!;

        public int? UserId { get; set; }

        public bool IsTemporary { get; set; } = true;

        public int? MergedIntoUserId { get; set; }

        public DateTime? MergedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}