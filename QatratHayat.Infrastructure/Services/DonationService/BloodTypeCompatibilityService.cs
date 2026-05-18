using QatratHayat.Application.Features.Donations.Interfaces;
using QatratHayat.Domain.Enums;

namespace QatratHayat.Infrastructure.Services
{
    public class BloodTypeCompatibilityService : IBloodTypeCompatibilityService
    {
        public bool CanDonateTo(BloodType donorBloodType, BloodType recipientBloodType)
        {
            return donorBloodType switch
            {
                BloodType.ONegative => true,

                BloodType.OPositive =>
                    recipientBloodType is BloodType.OPositive
                    or BloodType.APositive
                    or BloodType.BPositive
                    or BloodType.ABPositive,

                BloodType.ANegative =>
                    recipientBloodType is BloodType.ANegative
                    or BloodType.APositive
                    or BloodType.ABNegative
                    or BloodType.ABPositive,

                BloodType.APositive =>
                    recipientBloodType is BloodType.APositive
                    or BloodType.ABPositive,

                BloodType.BNegative =>
                    recipientBloodType is BloodType.BNegative
                    or BloodType.BPositive
                    or BloodType.ABNegative
                    or BloodType.ABPositive,

                BloodType.BPositive =>
                    recipientBloodType is BloodType.BPositive
                    or BloodType.ABPositive,

                BloodType.ABNegative =>
                    recipientBloodType is BloodType.ABNegative
                    or BloodType.ABPositive,

                BloodType.ABPositive =>
                    recipientBloodType == BloodType.ABPositive,

                _ => false
            };
        }
    }
}