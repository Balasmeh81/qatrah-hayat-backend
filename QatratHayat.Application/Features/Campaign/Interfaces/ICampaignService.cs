using QatratHayat.Application.Common.DTOS;
using QatratHayat.Application.Features.Campaigns.DTOS;

namespace QatratHayat.Application.Features.Campaigns.Interfaces
{
    public interface ICampaignService
    {
        Task<PagedResultDto<CampaignResponseDto>> GetAllCampaignsAsync(
            CampaignQueryDto query,
            int currentUserId,
            bool isAdmin
        );

        Task<CampaignResponseDto> GetCampaignByIdAsync(
            int campaignId,
            int currentUserId,
            bool isAdmin
        );

        Task<CampaignStatisticsResponseDto> GetStatisticsAsync(
            int currentUserId,
            bool isAdmin
        );

        Task<CampaignResponseDto> CreateCampaignAsync(
            CreateCampaignRequestDto request,
            int createdByUserId,
            bool isAdmin
        );

        Task<CampaignResponseDto> UpdateCampaignAsync(
            int campaignId,
            UpdateCampaignRequestDto request,
            int currentUserId,
            bool isAdmin
        );

        Task SoftDeleteCampaignAsync(
            int campaignId,
            int currentUserId,
            bool isAdmin
        );

        Task ActivateCampaignAsync(
            int campaignId,
            int currentUserId,
            bool isAdmin
        );

        Task CompleteCampaignAsync(
            int campaignId,
            int currentUserId,
            bool isAdmin
        );

        Task CancelCampaignAsync(
            int campaignId,
            int currentUserId,
            bool isAdmin
        );
    }
}