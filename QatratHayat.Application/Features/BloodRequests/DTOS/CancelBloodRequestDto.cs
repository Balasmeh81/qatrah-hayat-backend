using System.ComponentModel.DataAnnotations;

namespace QatratHayat.Application.Features.BloodRequests.DTOS
{
    public class CancelBloodRequestDto
    {
        [Required]
        [MaxLength(500)]
        public string CancellationReason { get; set; } = null!;
    }
}