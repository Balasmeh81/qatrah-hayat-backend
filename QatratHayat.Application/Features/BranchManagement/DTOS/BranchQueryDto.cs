namespace QatratHayat.Application.Features.BranchManagement.DTOS
{
    public class BranchQueryDto
    {
        public string? SearchTerm { get; set; }

        public bool? IsActive { get; set; }

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;
    }
}