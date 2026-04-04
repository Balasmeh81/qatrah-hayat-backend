using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QatratHayat.Application.Accounts.DTOs;
using QatratHayat.Application.Accounts.Services;
using QatratHayat.Application.Common.Interfaces;
using QatratHayat.Domain.Entities;
using QatratHayat.Domain.Enums;
using QatratHayat.Infrastructure.Identity;
using QatratHayat.Infrastructure.Persistence;
using System.Security.Claims;

namespace QatratHayat.Infrastructure.Services
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly AppDbContext context;
        private readonly IJwtTokenService jwtTokenService;

        //Constructor
        public AccountService(
            UserManager<ApplicationUser> _userManager,
            AppDbContext _context,
            IJwtTokenService _jwtTokenService)
        {
            userManager = _userManager;
            context = _context;
            jwtTokenService = _jwtTokenService;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
        {
            //1. Confirm Password
            if (request.Password != request.ConfirmPassword)
                throw new Exception("Password and Confirm Password do not match.");

            //2. Check the national registry
            var registryRecord = await context.NationalRegistries
                .FirstOrDefaultAsync(x => x.NationalId == request.NationalId);

            if (registryRecord is null)
                throw new Exception("National ID was not found in National Registry.");

            if (!registryRecord.IsJordanian)
                throw new Exception("Only Jordanian citizens can register.");


            //3.Check if the email is already registered
            var existingUserByEmail = await userManager.FindByEmailAsync(request.Email);
            if (existingUserByEmail is not null)
                throw new Exception("Email is already registered.");

            //4. check duplicate national ID
            var existingUserByNationalId = await context.Users
                .FirstOrDefaultAsync(x => x.NationalId == request.NationalId);
            if (existingUserByNationalId is not null)
                throw new Exception("National ID is already registered.");


            //5. Create User
            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                NationalId = request.NationalId,
                FullNameAr = request.FullNameAr,
                FullNameEn = request.FullNameEn,
                DateOfBirth = request.DateOfBirth,
                PhoneNumber = request.PhoneNumber,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            };


            //6. Create an official user and add it on DB.
            var result = await userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(" | ", result.Errors.Select(x => x.Description));
                throw new Exception(errors);
            }


            //7. Give Role
            await userManager.AddToRoleAsync(user, UserRole.Citizen.ToString());


            //8. Create DonorProfile then merge the user to this DonorProfile.
            var donorProfile = new DonorProfile
            {
                UserId = user.Id,
                BloodType = request.BloodType,
                BloodTypeStatus = BloodTypeStatus.Provisional,
                EligibilityStatus = EligibilityStatus.Eligible,
                DonationCount = 0,
                CreatedAt = DateTime.UtcNow
            };

            await context.donorProfiles.AddAsync(donorProfile);
            await context.SaveChangesAsync();


            //9. Generate JWT
            var token = jwtTokenService.GenerateToken(
                user.Id,
                user.Email!,
                user.FullNameAr,
                user.FullNameEn,
                UserRole.Citizen,
                null,
                null);


            //10. Return a response object containing the user data and token
            return new AuthResponseDto
            {
                UserId = user.Id,
                Email = user.Email!,
                FullNameAr = user.FullNameAr,
                FullNameEn = user.FullNameEn,
                Role = UserRole.Citizen.ToString(),
                Token = token
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
        {
            //1.User search
            var user = await context.Users
                .FirstOrDefaultAsync(x =>
                    x.Email == request.EmailOrNationalId ||
                    x.NationalId == request.EmailOrNationalId);
            //2. Check if the user is not present
            if (user is null)
                throw new Exception("Invalid email/National ID or password.");
            //3. Verify that the account is active!?
            if (!user.IsActive || user.IsDeleted)
                throw new Exception("This account is inactive.");


            //4. Checks whether the password entered by the user is correct or not
            var passwordValid = await userManager.CheckPasswordAsync(user, request.Password);
            if (!passwordValid)
                throw new Exception("Invalid email/National ID or password.");


            //5. Bring the Role
            var roles = await userManager.GetRolesAsync(user);
            var roleName = roles.FirstOrDefault() ?? UserRole.Citizen.ToString();

            //6. Convert text to Enum
            Enum.TryParse<UserRole>(roleName, out var parsedRole);

            //7. Creating a JWT Token
            var token = jwtTokenService.GenerateToken(
                user.Id,
                user.Email!,
                user.FullNameAr,
                user.FullNameEn,
                parsedRole,
                user.BranchId,
                user.HospitalId);


            // 8) Return a response object containing the user data and token
            return new AuthResponseDto
            {
                UserId = user.Id,
                Email = user.Email!,
                FullNameAr = user.FullNameAr,
                FullNameEn = user.FullNameEn,
                Role = roleName,
                Token = token
            };
        }

        public async Task<CurrentUserDto> GetCurrentUserAsync(ClaimsPrincipal userPrincipal)
        {
            // 1) Read userId from token claims
            var userIdClaim = userPrincipal.FindFirst("userId")?.Value;

            if (string.IsNullOrWhiteSpace(userIdClaim))
                throw new Exception("User ID claim was not found in token.");

            if (!int.TryParse(userIdClaim, out int userId))
                throw new Exception("Invalid user ID in token.");

            // 2) Get user from database
            var user = await context.Users.FirstOrDefaultAsync(x => x.Id == userId);

            if (user is null)
                throw new Exception("User not found.");

            if (!user.IsActive || user.IsDeleted)
                throw new Exception("This account is inactive.");

            // 3) Get user role
            var roles = await userManager.GetRolesAsync(user);
            var roleName = roles.FirstOrDefault() ?? UserRole.Citizen.ToString();

            // 4) Return current user data
            return new CurrentUserDto
            {
                UserId = user.Id,
                Email = user.Email!,
                FullNameAr = user.FullNameAr,
                FullNameEn = user.FullNameEn,
                Role = roleName
            };
        }
    }
}