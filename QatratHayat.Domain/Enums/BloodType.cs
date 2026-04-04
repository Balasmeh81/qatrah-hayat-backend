namespace QatratHayat.Domain.Enums
{
    public enum BloodType
    {
        APositive = 1,
        ANegative = 2,
        BPositive = 3,
        BNegative = 4,
        ABPositive = 5,
        ABNegative = 6,
        OPositive = 7,
        ONegative = 8
    }

    public static class BloodTypeExtensions
    {
        public static string ToDisplayName(this BloodType bloodType)
        {
            return bloodType switch
            {
                BloodType.APositive => "A+",
                BloodType.ANegative => "A-",
                BloodType.BPositive => "B+",
                BloodType.BNegative => "B-",
                BloodType.ABPositive => "AB+",
                BloodType.ABNegative => "AB-",
                BloodType.OPositive => "O+",
                BloodType.ONegative => "O-",
                _ => "Unknown"
            };
        }
    }
}