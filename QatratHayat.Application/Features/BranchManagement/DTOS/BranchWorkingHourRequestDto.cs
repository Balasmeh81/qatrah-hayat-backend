using System.ComponentModel.DataAnnotations;

namespace QatratHayat.Application.Features.BranchManagement.DTOS
{
    public class BranchWorkingHourRequestDto
    {
        [Required]
        public DayOfWeek DayOfWeek { get; set; }

        [Required]
        public TimeSpan OpenTime { get; set; }

        [Required]
        public TimeSpan CloseTime { get; set; }

        [Required]
        public bool IsClosed { get; set; }
    }
}