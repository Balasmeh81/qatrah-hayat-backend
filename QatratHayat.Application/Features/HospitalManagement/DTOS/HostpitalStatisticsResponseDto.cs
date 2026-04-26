namespace QatratHayat.Application.Features.HospitalManagement.DTOS
{
    public class HospitalStatisticsResponseDto
    {
        public int TotalHospitals { get; set; }

        public int ActiveHospitals { get; set; }

        public int InactiveHospitals { get; set; }

        public DateTime? LastUpdate { get; set; }
    }
}
