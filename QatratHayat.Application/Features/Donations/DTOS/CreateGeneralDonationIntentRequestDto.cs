using System.ComponentModel.DataAnnotations;

namespace QatratHayat.Application.Features.Donations.DTOs
{
    public class CreateGeneralDonationIntentRequestDto
    {
        [Required]
        public int BranchId { get; set; }

        [Required]
        public int ScreeningSessionId { get; set; }
    }
}