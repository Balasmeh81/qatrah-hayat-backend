using Microsoft.AspNetCore.Identity;
using QatratHayat.Domain.Entities;
using QatratHayat.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace QatratHayat.Infrastructure.Identity
{
    public class ApplicationUser : IdentityUser<int>
    {
        [Required]
        [StringLength(10, MinimumLength = 10)]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "National ID must be exactly 10 digits.")]
        public string NationalId { get; set; } = null!;
        [Required]
        [MaxLength(256)]
        public string FullNameAr { get; set; } = null!;
        [Required]
        [MaxLength(256)]
        public string FullNameEn { get; set; } = null!;
        [Required]
        public DateTime DateOfBirth { get; set; }
        [Required]
        public Gender Gender { get; set; }
        [Required]
        public MaritalStatus MaritalStatus { get; set; }
        [Required]
        public bool IsActive { get; set; }
        [Required]
        public bool IsDeleted { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }
        [Required]
        public bool IsProfileCompleted { get; set; }

        public string? JobTitle { get; set; }
        public string? Address { get; set; }
        public DateTime? UpdatedAt { get; set; }


        // Navigation Property
        public int? HospitalId { get; set; }
        public Hospital? Hospital { get; set; }

        public int? BranchId { get; set; }
        public Branch? Branch { get; set; }

        public DonorProfile? DonorProfile { get; set; }

        public ICollection<BloodRequest> CreatedBloodRequests { get; set; } = new List<BloodRequest>();
        public ICollection<Donation> ProcessedDonations { get; set; } = new List<Donation>();
        public ICollection<Campaign> CreatedCampaigns { get; set; } = new List<Campaign>();
        public ICollection<ScreeningAnswer> ScreeningAnswers { get; set; } = new List<ScreeningAnswer>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public ICollection<ScreeningSession> ScreeningSessions { get; set; } = new List<ScreeningSession>();
    }
}