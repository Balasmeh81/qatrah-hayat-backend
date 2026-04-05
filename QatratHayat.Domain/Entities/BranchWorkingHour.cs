using System.ComponentModel.DataAnnotations;

namespace QatratHayat.Domain.Entities
{
    public class BranchWorkingHour
    {
        public int Id { get; set; }
        [Required]
        public DayOfWeek DayOfWeek { get; set; }
        [Required]
        public TimeSpan OpenTime { get; set; }
        [Required]
        public TimeSpan CloseTime { get; set; }
        [Required]
        public bool IsClosed { get; set; }

        // Navigation Property
        [Required]
        public int BranchId { get; set; }
        public Branch Branch { get; set; } = null!;
    }
}
