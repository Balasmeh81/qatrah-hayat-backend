using QatratHayat.Domain.Enums;

namespace QatratHayat.Application.Features.BloodRequests.DTOS
{
    public class BloodRequestDetailsResponseDto : BloodRequestResponseDto
    {
        public string? ClinicalNotes { get; set; }

        public string? CancellationReason { get; set; }

        public DateTime? CancelledAt { get; set; }

        public int? CancelledByUserId { get; set; }

        public string? RejectionReason { get; set; }

        public DateTime? RejectedAt { get; set; }

        public int? RejectedByUserId { get; set; }

        public int? PublishedByUserId { get; set; }

        public List<AllocatedBloodUnitDto> AllocatedBloodUnits { get; set; } = new();
    }

    public class AllocatedBloodUnitDto
    {
        public int Id { get; set; }

        public string UnitCode { get; set; } = null!;

        public BloodType BloodType { get; set; }

        public string BloodTypeDisplayName { get; set; } = null!;

        public UnitStatus UnitStatus { get; set; }

        public DateTime CollectionDate { get; set; }

        public DateTime ExpiresAt { get; set; }

        public DateTime? AllocatedAt { get; set; }
    }
}