namespace QatratHayat.Application.Features.BranchManagement.DTOS
{
    public class BranchWorkingHourResponseDto
    {
        public int Id { get; set; }

        public DayOfWeek DayOfWeek { get; set; }

        public TimeSpan OpenTime { get; set; }

        public TimeSpan CloseTime { get; set; }

        public bool IsClosed { get; set; }
    }
}