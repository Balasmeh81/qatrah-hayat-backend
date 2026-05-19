using QatratHayat.Domain.Enums;

namespace QatratHayat.Application.Features.Campaigns.DTOS
{
    public class CampaignQueryDto
    {
        public string? SearchTerm { get; set; }

        public CampaignStatus? Status { get; set; }

        public CampaignType? Type { get; set; }

        public int? BranchId { get; set; }

        public BloodType? BloodType { get; set; }

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;
    }
}