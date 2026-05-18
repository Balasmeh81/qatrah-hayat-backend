using System.ComponentModel.DataAnnotations;

namespace QatratHayat.Application.Features.Donations.DTOs
{
    public class CreateRequestDonationIntentRequestDto
    {
        [Required]
        public int BloodRequestId { get; set; }

        [Required]
        public int ScreeningSessionId { get; set; }
    }
}