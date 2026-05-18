namespace QatratHayat.Application.Features.Donations.Interfaces
{
    public interface IUnitCodeGenerator
    {
        Task<string> GenerateUniqueUnitCodeAsync();
    }
}