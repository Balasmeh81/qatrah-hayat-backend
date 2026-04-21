using QatratHayat.Domain.Enums;

namespace QatratHayat.Application.Features.UsersManagement.DTOS
{
    public class CitizenResponseDto
    {
        public string NationalId { get; set; } = null!;

        public string FullNameAr { get; set; } = null!;

        public string FullNameEn { get; set; } = null!;

        public DateTime DateOfBirth { get; set; }

        public BloodType BloodType { get; set; }

        public Gender Gender { get; set; }

        public bool IsUser { get; set; }

        public bool IsStaff { get; set; }

        public int? UserId { get; set; }
    }
}