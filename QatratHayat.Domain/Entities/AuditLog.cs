using System.ComponentModel.DataAnnotations;

namespace QatratHayat.Domain.Entities
{
    public class AuditLog
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(100)]
        public string Action { get; set; } = null!;
        [Required]
        [MaxLength(100)]
        public string EntityType { get; set; } = null!;
        [Required]
        public int EntityId { get; set; }
        [Required]
        public DateTime Timestamp { get; set; }

        [MaxLength(2000)]
        public string? OldValues { get; set; }
        [MaxLength(2000)]
        public string? NewValues { get; set; }
        [MaxLength(45)]
        public string? IPAddress { get; set; }

        // Navigation Property
        [Required]
        public int UserId { get; set; }

    }
}
