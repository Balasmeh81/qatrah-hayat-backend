using QatratHayat.Domain.Enums;

namespace QatratHayat.Application.Features.Donations.DTOs
{
    public class PublishedBloodRequestForDonationDto
    {
        public int BloodRequestId { get; set; }

        public string PatientNationalId { get; set; } = null!;
        public string PatientFullNameAr { get; set; } = null!;
        public string PatientFullNameEn { get; set; } = null!;

        public string? ContactPhoneNumber { get; set; }

        public BloodType BloodType { get; set; }
        public UrgencyLevel UrgencyLevel { get; set; }

        public int BranchId { get; set; }
        public string BranchNameAr { get; set; } = null!;
        public string BranchNameEn { get; set; } = null!;

        public int HospitalId { get; set; }
        public string HospitalNameAr { get; set; } = null!;
        public string HospitalNameEn { get; set; } = null!;

        public int UnitsNeeded { get; set; }
        public int UnitsAllocatedOrUsed { get; set; }
        public int UnitsRemaining { get; set; }

        public DateTime PublishedAt { get; set; }
    }
}