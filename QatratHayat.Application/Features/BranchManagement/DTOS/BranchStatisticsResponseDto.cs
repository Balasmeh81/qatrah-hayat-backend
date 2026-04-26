namespace QatratHayat.Application.Features.BranchManagement.DTOS
{
    public class BranchStatisticsResponseDto
    {
        public int TotalBranches { get; set; }
        public int ActiveBranches { get; set; }
        public int InactiveBranches { get; set; }
        public DateTime? LastUpdate { get; set; }
    }
}
