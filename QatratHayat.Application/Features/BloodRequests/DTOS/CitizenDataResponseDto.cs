using QatratHayat.Domain.Enums;

namespace QatratHayat.Application.Features.BloodRequests.DTOS
{
    public class CitizenDataResponseDto
    {
        public string NationalId { get; set; } = null!;
        public string FullNameAr { get; set; } = null!;
        public string FullNameEn { get; set; } = null!;
        public BloodType BloodType { get; set; }
        public string BloodTypeDisplayName { get; set; } = null!;
    }
}
