using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QatratHayat.Application.Common.Exceptions;
using QatratHayat.Application.Features.Accounts.DTOs;
using QatratHayat.Application.Features.Auth.DTOs;
using QatratHayat.Application.Features.Auth.Interfaces;
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
            IJwtTokenService _jwtTokenService
        )
        {
            userManager = _userManager;
            context = _context;
            jwtTokenService = _jwtTokenService;
        }

        //User Register
        public async Task<RegisterResponseDto> RegisterCitizenAsync(RegisterRequestDto request)
        {
            // Normalize important string inputs first to avoid problems caused by extra spaces.
            var email = request.Email.Trim();
            var nationalId = request.NationalId.Trim();

            //1. Check For Password
            if (request.Password != request.ConfirmPassword)
                throw new BadRequestException(
                    "Registration failed.",
                    ErrorCodes.ValidationError,
                    new List<string> { "Password and Confirm Password do not match." }
                );

            //2. Check The registryRecord
            var registryRecord = await context
                .NationalRegistries.AsNoTracking()
                .FirstOrDefaultAsync(x => x.NationalId == nationalId);

            if (registryRecord is null)
                throw new NotFoundException(
                    "National ID was not found in National Registry.",
                    ErrorCodes.NationalIdNotFound
                );

            if (!registryRecord.IsJordanian)
                throw new BadRequestException(
                    "Only Jordanian citizens can register.",
                    ErrorCodes.NonJordanianCitizen
                );
            //3. Check Duplicate NationalId
            var existingUserByNationalId = await context
                .Users.AsNoTracking()
                .FirstOrDefaultAsync(x => x.NationalId == nationalId);

            if (existingUserByNationalId is not null)
                throw new ConflictException(
                    "National ID is already registered.",
                    ErrorCodes.NationalIdAlreadyRegistered
                );

            //4. Check If The Email Is Alrady Registed
            // Using UserManager here is better for email because Identity handles normalized email values.
            var existingUserByEmail = await userManager.FindByEmailAsync(email);
            if (existingUserByEmail is not null)
                throw new ConflictException(
                    "Email is already registered.",
                    ErrorCodes.EmailAlreadyRegistered
                );

            //5. Create User
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                NationalId = nationalId,
                FullNameAr = request.FullNameAr.Trim(),
                FullNameEn = request.FullNameEn.Trim(),
                DateOfBirth = request.DateOfBirth,
                Gender = request.Gender,
                PhoneNumber = request.PhoneNumber.Trim(),
                Address = request.Address,
                JobTitle = request.JobTitle,
                MaritalStatus = request.MaritalStatus,
                IsActive = true,
                IsDeleted = false,
                IsProfileCompleted = false,
                CreatedAt = DateTime.UtcNow,
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
                    ErrorCodes.RegistrationFailed,
                    createResult.Errors.Select(x => x.Description).ToList()
                );
            }

            //7. Give The User Role
            var roleResult = await userManager.AddToRoleAsync(user, UserRole.Citizen.ToString());

            if (!roleResult.Succeeded)
            {
                throw new BadRequestException(
                    "Failed to assign user role.",
                    ErrorCodes.RoleAssignmentFailed,
                    roleResult.Errors.Select(x => x.Description).ToList()
                );
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
                iAgree = request.iAgree,
                iConfirm = request.iConfirm,
            };

            await context.DonorProfiles.AddAsync(donorProfile);
            await context.SaveChangesAsync();

            await transaction.CommitAsync();

            //9. Returne RegisterResponseDto
            return new RegisterResponseDto
            {
                UserId = user.Id,
                Email = user.Email!,
                FullNameAr = user.FullNameAr,
                FullNameEn = user.FullNameEn,
                Message = "Account created successfully. Please log in.",
            };
        }

        public async Task<ApplicationUser?> GetUserAsync(string nationalId)
        {
            var normalizedInput = nationalId.Trim();
            ApplicationUser? user;
            //1. Searech For User
            user = await context.Users.FirstOrDefaultAsync(x => x.NationalId == normalizedInput);
            //2. Check If User Found
            if (user is null)
                return null;

            return user;
        }

        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
        {
            var normalizedInput = request.NationalId.Trim();

            ApplicationUser? user;
            // 1. Get User
            user = await GetUserAsync(normalizedInput);
            // 2.Check User
            if (user is null)
                throw new UnauthorizedException(
                    "Invalid email/National ID or password.",
                    ErrorCodes.AuthInvalidCredentials
                );

            //3. Check User Is Active
            if (!user.IsActive || user.IsDeleted)
                throw new UnauthorizedException(
                    "This account is inactive.",
                    ErrorCodes.AuthAccountInactive
                );

            //4. Check password Valid
            var passwordValid = await userManager.CheckPasswordAsync(user, request.Password);
            if (!passwordValid)
                throw new UnauthorizedException(
                    "Invalid email/National ID or password.",
                    ErrorCodes.AuthInvalidCredentials
                );

            //5. Bring The Role
            var roleNames = await userManager.GetRolesAsync(user);

            // check roleNames if is null
            if (roleNames is null || roleNames.Count == 0)
                throw new BadRequestException(
                    "User role is not assigned.",
                    ErrorCodes.UserRoleNotAssigned
                );
            //6. Convert Role Names To UserRole Enum
            var parsedRoles = new List<UserRole>();

            foreach (var roleName in roleNames)
            {
                if (!Enum.TryParse<UserRole>(roleName, out var parsedRole))
                    throw new BadRequestException(
                        $"User role '{roleName}' is invalid.",
                        ErrorCodes.UserRoleInvalid
                    );

                parsedRoles.Add(parsedRole);
            }

            //7. Bring The bloodType
            var donorProfile = await context
                .DonorProfiles.AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserId == user.Id);

            if (donorProfile is null)
                throw new NotFoundException(
                    "Donor profile not found.",
                    ErrorCodes.DonorProfileNotFound
                );

            BloodType bloodType = donorProfile.BloodType;

            //8. Generate JWT
            var token = jwtTokenService.GenerateToken(
                user.Id,
                user.Email!,
                user.FullNameAr,
                user.FullNameEn,
                parsedRoles,
                user.BranchId,
                user.HospitalId
            );

            //9. Returne AuthResponse
            return new LoginResponseDto
            {
                UserId = user.Id,
                Email = user.Email!,
                FullNameAr = user.FullNameAr,
                FullNameEn = user.FullNameEn,
                Roles = parsedRoles,
                DateOfBirth = user.DateOfBirth,
                BloodType = bloodType,
                Gender = user.Gender,
                IsProfileCompleted = user.IsProfileCompleted,
                Token = token,
            };
        }

        public async Task<CurrentUserDto> GetCurrentUserAsync(ClaimsPrincipal userPrincipal)
        {
            //1. Read User Id From Token Claims
            var userIdClaim = userPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userIdClaim))
                throw new UnauthorizedException(
                    "User ID claim was not found in token.",
                    ErrorCodes.AuthMissingUserIdClaim
                );

            if (!int.TryParse(userIdClaim, out int userId))
                throw new UnauthorizedException(
                    "Invalid user ID in token.",
                    ErrorCodes.AuthInvalidUserIdClaim
                );

            //2. Get User From DB
            var user = await context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == userId);

            if (user is null)
                throw new NotFoundException("User not found.", ErrorCodes.UserNotFound);

            if (!user.IsActive || user.IsDeleted)
                throw new UnauthorizedException(
                    "This account is inactive.",
                    ErrorCodes.AuthAccountInactive
                );

            //3. Bring The Role
            var roleNames = await userManager.GetRolesAsync(user);

            // check roleNames if is null
            if (roleNames is null || roleNames.Count == 0)
                throw new BadRequestException(
                    "User role is not assigned.",
                    ErrorCodes.UserRoleNotAssigned
                );
            //4. Convert Role Names To UserRole Enum
            var parsedRoles = new List<UserRole>();

            foreach (var roleName in roleNames)
            {
                if (!Enum.TryParse<UserRole>(roleName, out var parsedRole))
                    throw new BadRequestException(
                        $"User role '{roleName}' is invalid.",
                        ErrorCodes.UserRoleInvalid
                    );

                parsedRoles.Add(parsedRole);
            }
            //5. Bring The bloodType
            var donorProfile = await context
                .DonorProfiles.AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserId == user.Id);

            if (donorProfile is null)
                throw new NotFoundException(
                    "Donor profile not found.",
                    ErrorCodes.DonorProfileNotFound
                );

            BloodType bloodType = donorProfile.BloodType;

            //6. Return CurrentUser Date
            return new CurrentUserDto
            {
                UserId = user.Id,
                Email = user.Email!,
                FullNameAr = user.FullNameAr,
                FullNameEn = user.FullNameEn,
                Roles = parsedRoles,
                DateOfBirth = user.DateOfBirth,
                BloodType = bloodType,
                Gender = user.Gender,
                IsProfileCompleted = user.IsProfileCompleted,
            };
        }
    }
}
