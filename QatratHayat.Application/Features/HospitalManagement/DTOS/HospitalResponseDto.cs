namespace QatratHayat.Application.Features.HospitalManagement.DOTS
{
    public class HospitalResponseDto
    {
        public int Id { get; set; }
        public string HospitalNameAr { get; set; } = null!;
        public string HospitalNameEn { get; set; } = null!;
        public string AddressAR { get; set; } = null!;
        public string AddressEn { get; set; } = null!;
        public bool IsActive { get; set; }

        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
        public int BranchId { get; set; }
        public string? BranchNameAr { get; set; }

        public string? BranchNameEn { get; set; }
    }
}
