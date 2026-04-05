using System.ComponentModel.DataAnnotations;
using QatratHayat.Domain.Enums;

namespace QatratHayat.Domain.Entities
{
    public class Campaign
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(256)]
        public string TitleAr { get; set; } = null!;
        [Required]
        [MaxLength(256)]
        public string TitleEn { get; set; } = null!;
        [Required]
        [MaxLength(2000)]
        public string DescriptionAr { get; set; }= null!;
        [Required]
        [MaxLength(2000)]
        public string DescriptionEn { get; set; }=null!;
        [Required]
        public CampaignType Type { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
        [Required]
        public CampaignStatus Status { get; set; }
        [Required]
        public bool IsDeleted { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }
       

        [MaxLength(300)]
        public string? Location { get; set; }   
        public DateTime? UpdatedAt { get; set; }

        // Navigation Property
        public ICollection<CampaignTargetBloodType> TargetBloodTypes { get; set; } = new List<CampaignTargetBloodType>();
        public ICollection<DonationIntent> DonationIntents { get; set; } = new List<DonationIntent>();
        public ICollection<Donation> Donations { get; set; } = new List<Donation>();

        [Required]
        public int CreatedByUserId { get; set; }

        public int? BranchId { get; set; }
        public Branch? Branch { get; set; }
    }
}