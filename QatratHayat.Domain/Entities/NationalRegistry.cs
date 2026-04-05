using QatratHayat.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace QatratHayat.Domain.Entities
{
    
    public class NationalRegistry
    {

        public int Id { get; set; }
        [Required]
        [StringLength(10, MinimumLength = 10)]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "National ID must be exactly 10 digits.")]
        public string NationalId { get; set; } = null!;
        [Required]
        [MaxLength(256)]
        public string FullNameAr { get; set; } = null!;
        [Required]
        [MaxLength(256)]
        public string  FullNameEn { get; set; }=null!;
        [Required]
        public DateTime DateOfBirth { get; set; }
        [Required]
        public Gender Gender { get; set; }
        [Required]
        public BloodType BloodType { get; set; }
        [Required]
        public bool IsJordanian { get; set; }
    }
}
