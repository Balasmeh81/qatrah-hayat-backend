using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QatratHayat.Application.Accounts.DTOs;
using QatratHayat.Application.Common.Exceptions;
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

        //User Register
        public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
        {
            // Normalize important string inputs first to avoid problems caused by extra spaces.
            var email = request.Email.Trim();
            var nationalId = request.NationalId.Trim();

            //1. Check For Password
            if (request.Password != request.ConfirmPassword)
                throw new BadRequestException(
                    "Registration failed.",
                    new List<string> { "Password and Confirm Password do not match." });

            //2. Check The registryRecord 
            var registryRecord = await context.NationalRegistries
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.NationalId == nationalId);

            if (registryRecord is null)
                throw new NotFoundException("National ID was not found in National Registry.");

            if (!registryRecord.IsJordanian)
                throw new BadRequestException("Only Jordanian citizens can register.");

            //3. Check If The Email Is Alrady Registed
            // Using UserManager here is better for email because Identity handles normalized email values.
            var existingUserByEmail = await userManager.FindByEmailAsync(email);
            if (existingUserByEmail is not null)
                throw new ConflictException("Email is already registered.");

            //4. Check Duplicate NationalId
            var existingUserByNationalId = await context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.NationalId == nationalId);

            if (existingUserByNationalId is not null)
                throw new ConflictException("National ID is already registered.");

            //5. Create User
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                NationalId = nationalId,
                FullNameAr = request.FullNameAr.Trim(),
                FullNameEn = request.FullNameEn.Trim(),
                DateOfBirth = request.DateOfBirth,
                Gender=request.Gender,
                PhoneNumber = request.PhoneNumber.Trim(),
                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            };

            // This transaction ensures that user creation, role assignment,
            // and donor profile creation are treated as one logical unit.
            using var transaction = await context.Database.BeginTransactionAsync();

            //6. Create Official User and Insert to DB
            var createResult = await userManager.CreateAsync(user, request.Password);

            if (!createResult.Succeeded)
            {
                throw new BadRequestException(
                    "Registration failed.",
                    createResult.Errors.Select(x => x.Description).ToList());
            }

            //7. Give The User Role
            var roleResult = await userManager.AddToRoleAsync(user, UserRole.Citizen.ToString());

            if (!roleResult.Succeeded)
            {
                throw new BadRequestException(
                    "Failed to assign user role.",
                    roleResult.Errors.Select(x => x.Description).ToList());
            }

            //8. Create Donor Profile
            var donorProfile = new DonorProfile
            {
                UserId = user.Id,
                BloodType = request.BloodType,
                BloodTypeStatus = BloodTypeStatus.Provisional,
                EligibilityStatus = EligibilityStatus.Eligible,
                DonationCount = 0,
                CreatedAt = DateTime.UtcNow,
                iAgree= request.iAgree,
                iConfirm= request.iConfirm,
            };

            await context.DonorProfiles.AddAsync(donorProfile);
            await context.SaveChangesAsync();

            await transaction.CommitAsync();

            //9. Generate JWT 
            var token = jwtTokenService.GenerateToken(
                user.Id,
                user.Email!,
                user.FullNameAr,
                user.FullNameEn,
                UserRole.Citizen,
                null,
                null);

            //10. Returne AuthResponse
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
            var normalizedInput = request.EmailOrNationalId.Trim();

            ApplicationUser? user;
            //1. Searech For User
            // If input contains '@', we treat it as email and let Identity resolve it.
            if (normalizedInput.Contains("@"))
            {
                user = await userManager.FindByEmailAsync(normalizedInput);
            }
            else
            {
                
                user = await context.Users
                    .FirstOrDefaultAsync(x => x.NationalId == normalizedInput);
            }

            //2. Check If User Found
            if (user is null)
                throw new UnauthorizedException("Invalid email/National ID or password.");

            //3. Check User Is Active
            if (!user.IsActive || user.IsDeleted)
                throw new UnauthorizedException("This account is inactive.");

            //4. Check password Valid
            var passwordValid = await userManager.CheckPasswordAsync(user, request.Password);
            if (!passwordValid)
                throw new UnauthorizedException("Invalid email/National ID or password.");

            //5. Bring The Role
            var roles = await userManager.GetRolesAsync(user);
            var roleName = roles.FirstOrDefault();

            if (string.IsNullOrWhiteSpace(roleName))
                throw new BadRequestException("User role is not assigned.");

            //6. Convert Role To String
            if (!Enum.TryParse<UserRole>(roleName, out var parsedRole))
                throw new BadRequestException("User role is invalid.");

            //7. Generate JWT 
            var token = jwtTokenService.GenerateToken(
                user.Id,
                user.Email!,
                user.FullNameAr,
                user.FullNameEn,
                parsedRole,
                user.BranchId,
                user.HospitalId);

            //8. Returne AuthResponse
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
            //1. Read User Id From Token Claims
            var userIdClaim = userPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userIdClaim))
                throw new UnauthorizedException("User ID claim was not found in token.");

            if (!int.TryParse(userIdClaim, out int userId))
                throw new UnauthorizedException("Invalid user ID in token.");

            //2. Get User From DB
            var user = await context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == userId);

            if (user is null)
                throw new NotFoundException("User not found.");

            if (!user.IsActive || user.IsDeleted)
                throw new UnauthorizedException("This account is inactive.");

            //3. Get User Role
            var roles = await userManager.GetRolesAsync(user);
            var roleName = roles.FirstOrDefault() ?? UserRole.Citizen.ToString();

            //4. Return CurrentUser Date
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