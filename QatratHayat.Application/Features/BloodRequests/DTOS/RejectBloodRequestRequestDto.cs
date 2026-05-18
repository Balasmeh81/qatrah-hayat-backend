using System.ComponentModel.DataAnnotations;

namespace QatratHayat.Application.Features.BloodRequests.DTOS
{
    public class RejectBloodRequestRequestDto
    {
        [Required]
        [MaxLength(500)]
        public string RejectionReason { get; set; } = null!;
    }
}