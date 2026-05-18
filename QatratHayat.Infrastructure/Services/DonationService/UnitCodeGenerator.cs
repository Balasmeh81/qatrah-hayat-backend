using Microsoft.EntityFrameworkCore;
using QatratHayat.Application.Features.Donations.Interfaces;
using QatratHayat.Infrastructure.Persistence;

namespace QatratHayat.Infrastructure.Services
{
    public class UnitCodeGenerator : IUnitCodeGenerator
    {
        private readonly AppDbContext _context;

        public UnitCodeGenerator(AppDbContext context)
        {
            _context = context;
        }

        public async Task<string> GenerateUniqueUnitCodeAsync()
        {
            for (var attempt = 0; attempt < 10; attempt++)
            {
                var randomPart = Random.Shared.Next(1000, 9999);
                var unitCode = $"BU{DateTime.UtcNow:yyMMddHHmmss}{randomPart}";

                var exists = await _context.BloodUnits.AnyAsync(x => x.UnitCode == unitCode);

                if (!exists)
                {
                    return unitCode;
                }
            }

            throw new InvalidOperationException("Could not generate a unique blood unit code.");
        }
    }
}