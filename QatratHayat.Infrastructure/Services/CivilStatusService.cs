using Microsoft.EntityFrameworkCore;
using QatratHayat.Application.Common.Exceptions;
using QatratHayat.Application.Features.Accounts.DTOs;
using QatratHayat.Application.Features.Auth.Interfaces;
using QatratHayat.Infrastructure.Persistence;

namespace QatratHayat.Infrastructure.Services
{
    public class CivilStatusService : ICivilStatusService
    {
        private readonly AppDbContext context;

        public CivilStatusService(AppDbContext _context)
        {
            context = _context;
        }

        public async Task<NationalRegistryResponseDto> GetNationalRegistryAsync(string nationalId)
        {
            if (string.IsNullOrWhiteSpace(nationalId))
                throw new BadRequestException(
                    "National ID is required.",
                    ErrorCodes.NationalIdRequired
                );

            nationalId = nationalId.Trim();

            var result = await context
                .NationalRegistries.AsNoTracking()
                .FirstOrDefaultAsync(x => x.NationalId == nationalId);

            if (result is null)
                throw new NotFoundException(
                    "National ID was not found in National Registry.",
                    ErrorCodes.NationalIdNotFound
                );

            return new NationalRegistryResponseDto
            {
                NationalId = result.NationalId,
                FullNameAr = result.FullNameAr,
                FullNameEn = result.FullNameEn,
                DateOfBirth = result.DateOfBirth,
                BloodType = result.BloodType,
                Gender = result.Gender,
            };
        }
    }
}
