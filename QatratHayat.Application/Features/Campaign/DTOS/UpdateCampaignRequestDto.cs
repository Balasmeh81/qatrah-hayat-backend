using QatratHayat.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace QatratHayat.Application.Features.Campaigns.DTOS
{
    public class UpdateCampaignRequestDto
    {
        [Required]
        [MaxLength(256)]
        public string TitleAr { get; set; } = null!;

        [Required]
        [MaxLength(256)]
        public string TitleEn { get; set; } = null!;

        [Required]
        [MaxLength(2000)]
        public string DescriptionAr { get; set; } = null!;

        [Required]
        [MaxLength(2000)]
        public string DescriptionEn { get; set; } = null!;

        [Required]
        public CampaignType Type { get; set; }

        [Required]
        public CampaignStatus Status { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [MaxLength(300)]
        public string? Location { get; set; }

        public int? BranchId { get; set; }

        [Required]
        public List<BloodType> TargetBloodTypes { get; set; } = new();
    }
}