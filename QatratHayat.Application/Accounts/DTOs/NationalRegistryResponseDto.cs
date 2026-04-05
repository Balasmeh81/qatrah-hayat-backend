using QatratHayat.Domain.Enums;
namespace QatratHayat.Application.Accounts.DTOs
{
    public class NationalRegistryResponseDto
    {

        public string NationalId { get; set; } = null!;

        public string FullNameAr { get; set; } = null!;

        public string FullNameEn { get; set; } = null!;

        public DateTime DateOfBirth { get; set; }

        public BloodType BloodType { get; set; }

        public Gender Gender { get; set; }
    }
}
