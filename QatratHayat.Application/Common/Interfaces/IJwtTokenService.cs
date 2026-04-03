using QatratHayat.Domain.Enums;

namespace QatratHayat.Application.Common.Interfaces
{
    public interface IJwtTokenService
    {
        string GenerateToken(
           int userId,
           string email,
           string fullNameAr,
           string fullNameEn,
           UserRole role,
           int? branchId,
           int? hospitalId);
    }
}
