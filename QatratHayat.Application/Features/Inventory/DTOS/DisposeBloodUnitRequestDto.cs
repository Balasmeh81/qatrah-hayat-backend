using System.ComponentModel.DataAnnotations;

namespace QatratHayat.Application.Features.Inventory.DTOs
{
    public class DisposeBloodUnitRequestDto
    {
        [Required]
        [MaxLength(500)]
        public string DisposalReason { get; set; } = null!;
    }
}