using QatratHayat.Application.Common.DTOS;

namespace QatratHayat.Application.Features.UsersManagement.DTOS
{
    public class StaffInfoResponseDto : BaseUserInfoDto
    {
        public string? HospitalNameAr { get; set; }
        public string? HospitalNameEn { get; set; }
        public string? BranchNameAr { get; set; }
        public string? BranchNameEn { get; set; }

    }
}
