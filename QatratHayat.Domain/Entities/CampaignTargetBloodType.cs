using QatratHayat.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QatratHayat.Domain.Entities
{
    public class CampaignTargetBloodType
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CampaignId { get; set; }

        [ForeignKey(nameof(CampaignId))]
        public Campaign Campaign { get; set; } = null!;
        public BloodType BloodType { get; set; }
    }
}