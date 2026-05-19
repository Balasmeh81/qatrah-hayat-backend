namespace QatratHayat.Application.Features.Campaigns.DTOS
{
    public class CampaignStatisticsResponseDto
    {
        public int TotalCampaigns { get; set; }

        public int PlannedCampaigns { get; set; }

        public int ActiveCampaigns { get; set; }

        public int CompletedCampaigns { get; set; }

        public int CancelledCampaigns { get; set; }

        public int InternalCampaigns { get; set; }

        public int ExternalCampaigns { get; set; }

        public DateTime? LastUpdate { get; set; }
    }
}