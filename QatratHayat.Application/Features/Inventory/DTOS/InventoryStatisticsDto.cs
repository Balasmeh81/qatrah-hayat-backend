namespace QatratHayat.Application.Features.Inventory.DTOs
{
    public class InventoryStatisticsDto
    {
        public int TotalUnits { get; set; }

        public int AvailableUnits { get; set; }

        public int ReservedUnits { get; set; }

        public int AllocatedUnits { get; set; }

        public int UsedUnits { get; set; }

        public int ExpiredUnits { get; set; }

        public int DisposedUnits { get; set; }

        public int ExpiringSoonUnits { get; set; }

        public List<BloodTypeInventorySummaryDto> BloodTypes { get; set; } = [];
    }
}