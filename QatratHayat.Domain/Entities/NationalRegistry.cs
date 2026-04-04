namespace QatratHayat.Domain.Entities
{
    
    public class NationalRegistry
    {
        public int Id { get; set; }
        public string NationalId { get; set; } = null!;
        public string FullNameAr { get; set; } = null!;
        public string  FullNameEn { get; set; }=null!;
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; } = null!;
        public string BloodType { get; set; }=null!;
        public bool IsJordanian { get; set; }
    }
}
