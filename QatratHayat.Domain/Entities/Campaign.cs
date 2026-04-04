using System.ComponentModel.DataAnnotations;
using QatratHayat.Domain.Enums;

namespace QatratHayat.Domain.Entities
{
    public class Campaign
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = null!;

        [MaxLength(2000)]
        public string? Description { get; set; }

        [Required]
        public CampaignType Type { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [MaxLength(300)]
        public string? Location { get; set; }

        public int? BranchId { get; set; }

        public int CreatedByUserId { get; set; }

        [Required]
        public CampaignStatus Status { get; set; } = CampaignStatus.Planned;

        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}