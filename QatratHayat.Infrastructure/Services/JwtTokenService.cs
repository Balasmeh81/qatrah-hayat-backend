using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using QatratHayat.Application.Common.Interfaces;
using QatratHayat.Domain.Enums;

namespace QatratHayat.Infrastructure.Identity
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly IConfiguration _configuration;

        public JwtTokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(
            int userId,
            string email,
            string fullNameAr,
            string fullNameEn,
            UserRole role,
            int? branchId,
            int? hospitalId)
        {
            var jwtSection = _configuration.GetSection("Jwt");

            var key = jwtSection["Key"]!;
            var issuer = jwtSection["Issuer"]!;
            var audience = jwtSection["Audience"]!;
            var durationInMinutes = int.Parse(jwtSection["DurationInMinutes"]!);

            var claims = new List<Claim>
            {
                new Claim("userId", userId.ToString()),
                new Claim(ClaimTypes.Email, email),
                new Claim("fullNameAr", fullNameAr),
                new Claim("fullNameEn", fullNameEn),
                new Claim("role", role.ToString()),
                new Claim(ClaimTypes.Role, role.ToString())
            };

            if (branchId.HasValue)
                claims.Add(new Claim("branchId", branchId.Value.ToString()));

            if (hospitalId.HasValue)
                claims.Add(new Claim("hospitalId", hospitalId.Value.ToString()));

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(durationInMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}