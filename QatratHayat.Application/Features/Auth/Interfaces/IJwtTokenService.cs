using QatratHayat.Domain.Enums;

namespace QatratHayat.Application.Features.Auth.Interfaces
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
