namespace QatratHayat.Application.Features.BranchManagement.DTOS
{
    public class BranchResponseDto
    {
        public int Id { get; set; }

        public string BranchNameAr { get; set; } = null!;

        public string BranchNameEn { get; set; } = null!;

        public string AddressAr { get; set; } = null!;

        public string AddressEn { get; set; } = null!;

        public int ManagerUserId { get; set; }

        public string? ManagerFullNameAr { get; set; }

        public string? ManagerFullNameEn { get; set; }

        public bool IsActive { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime CreatedAt { get; set; }

        public decimal GPSLat { get; set; }

        public decimal GPSLng { get; set; }

        public string? Email { get; set; }

        public string? Phone { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}