using QatratHayat.Domain.Enums;

namespace QatratHayat.Application.Features.BloodRequests.DTOS
{
    public class BloodRequestResponseDto
    {
        public int Id { get; set; }

        public RelationshipType RelationshipType { get; set; }

        public BloodType? BloodType { get; set; }

        public string? BloodTypeDisplayName { get; set; }

        public int? UnitsNeeded { get; set; }

        public int UnitsReserved { get; set; }

        public int UnitsAllocated { get; set; }

        public int UnitsRemaining { get; set; }

        public UrgencyLevel? UrgencyLevel { get; set; }

        public RequestStatus RequestStatus { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? DoctorApprovedAt { get; set; }

        public DateTime? PublishedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public int BeneficiaryId { get; set; }

        public string BeneficiaryNationalId { get; set; } = null!;

        public string BeneficiaryFullNameAr { get; set; } = null!;

        public string BeneficiaryFullNameEn { get; set; } = null!;

        public int HospitalId { get; set; }

        public string HospitalNameAr { get; set; } = null!;

        public string HospitalNameEn { get; set; } = null!;

        public int BranchId { get; set; }

        public string BranchNameAr { get; set; } = null!;

        public string BranchNameEn { get; set; } = null!;

        public int RequesterUserId { get; set; }

        public string RequesterFullNameAr { get; set; } = null!;

        public string RequesterFullNameEn { get; set; } = null!;

        public int DoctorId { get; set; }

        public string DoctorFullNameAr { get; set; } = null!;

        public string DoctorFullNameEn { get; set; } = null!;
    }
}