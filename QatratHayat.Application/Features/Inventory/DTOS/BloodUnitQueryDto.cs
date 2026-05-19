using QatratHayat.Domain.Enums;

namespace QatratHayat.Application.Features.Inventory.DTOs
{
    public class BloodUnitQueryDto
    {
        public string? SearchTerm { get; set; }

        public BloodType? BloodType { get; set; }

        public UnitStatus? UnitStatus { get; set; }

        public int? BranchId { get; set; }

        public DateTime? FromCollectionDate { get; set; }

        public DateTime? ToCollectionDate { get; set; }

        public DateTime? ExpiringBefore { get; set; }

        public bool? ExpiredOnly { get; set; }

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;
    }
}