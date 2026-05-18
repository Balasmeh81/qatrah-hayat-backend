using QatratHayat.Domain.Enums;

namespace QatratHayat.Application.Features.Donations.Interfaces
{
    public interface IBloodTypeCompatibilityService
    {
        bool CanDonateTo(BloodType donorBloodType, BloodType recipientBloodType);
    }
}