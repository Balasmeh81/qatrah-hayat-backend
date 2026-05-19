using System.ComponentModel.DataAnnotations;

namespace QatratHayat.Application.Features.Inventory.DTOs
{
    public class ReturnBloodUnitToAvailableRequestDto
    {
        [Required]
        [MaxLength(500)]
        public string DeallocationNote { get; set; } = null!;
    }
}