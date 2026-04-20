namespace QatratHayat.Application.Features.HospitalManagement.DTOS
{
    public class HospitalQueryDto
    {
        public string? SearchTerm { get; set; }

        public bool? IsActive { get; set; }

        public int? BranchId { get; set; }

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;
    }
}