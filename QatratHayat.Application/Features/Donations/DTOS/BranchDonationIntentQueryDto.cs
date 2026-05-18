using QatratHayat.Domain.Enums;

namespace QatratHayat.Application.Features.Donations.DTOs
{
    public class BranchDonationIntentQueryDto
    {
        public DonationIntentStatus? Status { get; set; }
        public DonationType? DonationType { get; set; }
        public BloodType? BloodType { get; set; }

        public string? Search { get; set; }

        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}