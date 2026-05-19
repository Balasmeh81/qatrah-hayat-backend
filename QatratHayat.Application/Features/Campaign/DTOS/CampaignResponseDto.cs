using QatratHayat.Domain.Enums;

namespace QatratHayat.Application.Features.Campaigns.DTOS
{
    public class CampaignResponseDto
    {
        public int Id { get; set; }

        public string TitleAr { get; set; } = null!;

        public string TitleEn { get; set; } = null!;

        public string DescriptionAr { get; set; } = null!;

        public string DescriptionEn { get; set; } = null!;

        public CampaignType Type { get; set; }

        public CampaignStatus Status { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public string? Location { get; set; }

        public int CreatedByUserId { get; set; }

        public int? BranchId { get; set; }

        public string? BranchNameAr { get; set; }

        public string? BranchNameEn { get; set; }

        public List<BloodType> TargetBloodTypes { get; set; } = new();

        public int DonationIntentsCount { get; set; }

        public int DonationsCount { get; set; }
    }
}