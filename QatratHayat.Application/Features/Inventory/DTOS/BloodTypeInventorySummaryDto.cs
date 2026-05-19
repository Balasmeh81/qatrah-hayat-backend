using QatratHayat.Domain.Enums;

namespace QatratHayat.Application.Features.Inventory.DTOs
{
    public class BloodTypeInventorySummaryDto
    {
        public BloodType BloodType { get; set; }

        public string BloodTypeDisplayName { get; set; } = null!;

        public int AvailableCount { get; set; }

        public int PartiallyAllocatedCount { get; set; }

        public int AllocatedCount { get; set; }

        public int UsedCount { get; set; }

        public int ExpiredCount { get; set; }

        public int DisposedCount { get; set; }

        public int TotalCount { get; set; }

        public int ExpiringSoonCount { get; set; }
    }
}