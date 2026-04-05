using QatratHayat.Domain.Enums;

namespace QatratHayat.Domain.Entities
{
    public class CampaignTargetBloodType
    {
        public int Id { get; set; }
        public BloodType BloodType { get; set; }

        // Navigation Property
        public int CampaignId { get; set; }
        public Campaign Campaign { get; set; } = null!;
    }
}
