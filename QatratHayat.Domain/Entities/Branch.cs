using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QatratHayat.Domain.Entities
{
    public class Branch
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = null!;

        [Required]
        [MaxLength(500)]
        public string Address { get; set; } = null!;

        [Column(TypeName = "decimal(9,6)")]
        public decimal? GPSLat { get; set; }

        [Column(TypeName = "decimal(9,6)")]
        public decimal? GPSLng { get; set; }

        [MaxLength(200)]
        public string? ContactInfo { get; set; }

        public int? ManagerUserId { get; set; }

        [MaxLength(1000)]
        public string? WorkingHours { get; set; }

        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}