using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using QatratHayat.Application.Features.Auth.Interfaces;
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
            // Read JWT settings from appsettings.json
            var jwtSection = _configuration.GetSection("Jwt");

            var key = jwtSection["Key"]!;
            var issuer = jwtSection["Issuer"]!;
            var audience = jwtSection["Audience"]!;
            var durationInMinutes = int.Parse(jwtSection["DurationInMinutes"]!);

            var claims = new List<Claim>
            {
                // Standard claim for the authenticated user's unique identifier.
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                  // Standard email claim.
                new Claim(ClaimTypes.Email, email),
                  // Custom claims for displaying user information in the app if needed.
                new Claim("fullNameAr", fullNameAr),
                new Claim("fullNameEn", fullNameEn),

                // Keep role claim for ASP.NET Core authorization.
                new Claim(ClaimTypes.Role, role.ToString())
            };
            // Add branchId only if the user belongs to a branch.
            if (branchId.HasValue)
                claims.Add(new Claim("branchId", branchId.Value.ToString()));
            // Add hospitalId only if the user belongs to a hospital.
            if (hospitalId.HasValue)
                claims.Add(new Claim("hospitalId", hospitalId.Value.ToString()));
            // Convert secret key string into a cryptographic security key.
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            // Define signing algorithm and credentials.
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            // Build the JWT token with issuer, audience, claims, expiration, and signature.
            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(durationInMinutes),
                signingCredentials: credentials);
            // Convert token object to string form and return it.
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}