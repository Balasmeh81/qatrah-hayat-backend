

using QatratHayat.Domain.Enums;

namespace QatratHayat.Application.Features.Auth.DTOs
{
    public class UserInfoDto
    {
        public int UserId { get; set; }
        public string Email { get; set; } = null!;
        public string FullNameAr { get; set; } = null!;
        public string FullNameEn { get; set; } = null!;
        public List<UserRole> Roles { get; set; } = null!;
        public Gender Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
        public BloodType BloodType { get; set; }
        public int? BranchId { get; set; }
        public int? HospitalId { get; set; }
        public bool IsProfileCompleted { get; set; }
    }
}
