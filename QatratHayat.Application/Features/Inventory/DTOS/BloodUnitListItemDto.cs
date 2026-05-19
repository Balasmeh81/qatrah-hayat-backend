using QatratHayat.Domain.Enums;

namespace QatratHayat.Application.Features.Inventory.DTOs
{
    public class BloodUnitListItemDto
    {
        public int Id { get; set; }

        public string UnitCode { get; set; } = null!;

        public BloodType BloodType { get; set; }

        public string BloodTypeDisplayName { get; set; } = null!;

        public UnitStatus UnitStatus { get; set; }

        public DateTime CollectionDate { get; set; }

        public DateTime ExpiresAt { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? AllocatedAt { get; set; }

        public int BranchId { get; set; }

        public string BranchNameAr { get; set; } = null!;

        public string BranchNameEn { get; set; } = null!;

        public int DonationId { get; set; }

        public int? AllocatedToRequestId { get; set; }

        public bool IsExpiredByDate { get; set; }

        public int DaysUntilExpiry { get; set; }
    }
}