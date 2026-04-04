using Microsoft.AspNetCore.Identity;
using QatratHayat.Domain.Enums;

namespace QatratHayat.Infrastructure.Identity
{
    public class ApplicationUser : IdentityUser<int>
    {
        public string NationalId { get; set; } = null!;
        public string FullNameAr { get; set; } = null!;
        public string FullNameEn { get; set; } = null!;
        public DateTime DateOfBirth { get; set; }
        public Gender  Gender { get; set; }
        public int? BranchId { get; set; }
        public int? HospitalId { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}