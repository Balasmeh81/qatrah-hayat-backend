using QatratHayat.Domain.Enums;

namespace QatratHayat.Application.Features.Donations.DTOs
{
    public class PublishedBloodRequestsForDonationQueryDto
    {
        public string? SearchTerm { get; set; }
        public UrgencyLevel? UrgencyLevel { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}