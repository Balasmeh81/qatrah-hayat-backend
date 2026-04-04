using QatratHayat.Domain.Enums;

namespace QatratHayat.Domain.Entities
{
    
    public class NationalRegistry
    {
        public int Id { get; set; }
        public string NationalId { get; set; } = null!;
        public string FullNameAr { get; set; } = null!;
        public string  FullNameEn { get; set; }=null!;
        public DateTime DateOfBirth { get; set; }
        public Gender Gender { get; set; } 
        public BloodType BloodType { get; set; }
        public bool IsJordanian { get; set; }
    }
}
