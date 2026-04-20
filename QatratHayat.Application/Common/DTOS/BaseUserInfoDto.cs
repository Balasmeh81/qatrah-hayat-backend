using QatratHayat.Domain.Enums;

namespace QatratHayat.Application.Common.DTOS
{
    public class BaseUserInfoDto
    {
        public int UserId { get; set; }
        public string NationalId { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string FullNameAr { get; set; } = null!;
        public string FullNameEn { get; set; } = null!;
        public List<UserRole> Roles { get; set; } = new();
        public Gender Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
        public BloodType BloodType { get; set; }
        public int? BranchId { get; set; }
        public int? HospitalId { get; set; }
        public bool IsProfileCompleted { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string PhoneNumber { get; set; } = null!;
    }
}
