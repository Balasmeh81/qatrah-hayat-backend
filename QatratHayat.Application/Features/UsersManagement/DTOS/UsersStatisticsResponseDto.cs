namespace QatratHayat.Application.Features.UsersManagement.DTOS
{
    public class UsersStatisticsResponseDto
    {
        public int TotalUsers { get; set; }
        public int TotalStaff { get; set; }
        public int TotalCitizens { get; set; }
        public DateTime? LastUpdate { get; set; }

    }
}
