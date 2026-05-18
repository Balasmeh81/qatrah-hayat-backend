using QatratHayat.Domain.Entities;

namespace QatratHayat.Application.Features.Donations.Interfaces
{
    public interface IBloodUnitSmartAllocationService
    {
        Task AllocateBloodUnitAsync(BloodUnit bloodUnit, Donation donation);
    }
}