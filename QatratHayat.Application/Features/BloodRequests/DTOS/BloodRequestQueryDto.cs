using QatratHayat.Domain.Enums;

namespace QatratHayat.Application.Features.BloodRequests.DTOS
{
    public class BloodRequestQueryDto
    {
        public string? SearchTerm { get; set; }

        public RequestStatus? RequestStatus { get; set; }

        public BloodType? BloodType { get; set; }

        public UrgencyLevel? UrgencyLevel { get; set; }

        public int? HospitalId { get; set; }

        public int? BranchId { get; set; }

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;
    }
}