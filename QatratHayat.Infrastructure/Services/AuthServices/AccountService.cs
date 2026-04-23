using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QatratHayat.Application.Common.Exceptions;
using QatratHayat.Application.Common.Interfaces;
using QatratHayat.Application.Features.Accounts.DTOs;
using QatratHayat.Application.Features.Auth.DTOs;
using QatratHayat.Application.Features.Auth.Interfaces;
using QatratHayat.Domain.Entities;
using QatratHayat.Domain.Enums;
using QatratHayat.Infrastructure.Identity;
using QatratHayat.Infrastructure.Persistence;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace QatratHayat.Infrastructure.Services
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly AppDbContext context;
        private readonly IJwtTokenService jwtTokenService;
        private readonly IEmailService emailService;

        public AccountService(
            UserManager<ApplicationUser> _userManager,
            AppDbContext _context,
            IJwtTokenService _jwtTokenService,
            IEmailService _emailService
        )
        {
            userManager = _userManager;
            context = _context;
            jwtTokenService = _jwtTokenService;
            emailService = _emailService;
        }

        //User Register
        public async Task<RegisterResponseDto> RegisterCitizenAsync(RegisterRequestDto request)
        {
            // Normalize important string inputs first to avoid problems caused by extra spaces.
            var email = request.Email.Trim();
            var nationalId = request.NationalId.Trim();
            if (!request.iAgree || !request.iConfirm)
            {
                throw new BadRequestException(
                    "Terms agreement and information confirmation are required.",
                    ErrorCodes.TermsAndConditionsRequired
                );
            }

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

            // 5. Check if the phone number is already registered
            var phoneNumber = request.PhoneNumber.Trim();

            var existingUserByPhone = await context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.PhoneNumber == phoneNumber);

            if (existingUserByPhone is not null)
            {
                throw new ConflictException(
                    "Phone number is already registered.",
                    ErrorCodes.PhoneNumberAlreadyRegistered
                );
            }
            //6. Create User
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                NationalId = nationalId,
                DateOfBirth = registryRecord.DateOfBirth,
                Gender = registryRecord.Gender,
                FullNameAr = registryRecord.FullNameAr,
                FullNameEn = registryRecord.FullNameEn,
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

            //7. Create Official User and Insert to DB
            var createResult = await userManager.CreateAsync(user, request.Password);

            if (!createResult.Succeeded)
            {
                throw new BadRequestException(
                    "Registration failed.",
                    ErrorCodes.RegistrationFailed,
                    createResult.Errors.Select(x => x.Description).ToList()
                );
            }

            //8. Give The User Role
            var roleResult = await userManager.AddToRoleAsync(user, UserRole.Citizen.ToString());

            if (!roleResult.Succeeded)
            {
                throw new BadRequestException(
                    "Failed to assign user role.",
                    ErrorCodes.RoleAssignmentFailed,
                    roleResult.Errors.Select(x => x.Description).ToList()
                );
            }

            //9. Create Donor Profile
            var donorProfile = new DonorProfile
            {
                UserId = user.Id,
                BloodType = registryRecord.BloodType,
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

            //10. Returne RegisterResponseDto
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
                    "Invalid National ID or password.",
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

            //6. Convert Role Names To UserRole Enum
            var parsedRoles = ParseRoles(roleNames);

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
                user.HospitalId,
                request.RememberMe
            );

            //9. Returne AuthResponse
            return new LoginResponseDto
            {
                UserId = user.Id,
                NationalId = user.NationalId,
                Email = user.Email!,
                FullNameAr = user.FullNameAr,
                FullNameEn = user.FullNameEn,
                Roles = parsedRoles,
                DateOfBirth = user.DateOfBirth,
                BloodType = donorProfile.BloodType,
                Gender = user.Gender,
                BranchId = user.BranchId,
                HospitalId = user.HospitalId,
                IsProfileCompleted = user.IsProfileCompleted,
                IsActive = user.IsActive,
                IsDeleted = user.IsDeleted,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                Token = token
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

            //4. Convert Role Names To UserRole Enum
            var parsedRoles = ParseRoles(roleNames);
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
                NationalId = user.NationalId,
                Email = user.Email!,
                FullNameAr = user.FullNameAr,
                FullNameEn = user.FullNameEn,
                Roles = parsedRoles,
                DateOfBirth = user.DateOfBirth,
                BloodType = donorProfile.BloodType,
                Gender = user.Gender,
                BranchId = user.BranchId,
                HospitalId = user.HospitalId,
                IsProfileCompleted = user.IsProfileCompleted,
                IsActive = user.IsActive,
                IsDeleted = user.IsDeleted,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                PhoneNumber = user.PhoneNumber ?? string.Empty
            };
        }

        //forgot password
        public async Task<ForgotPasswordMessageResponseDto> ForgotPasswordAsync(ForgotPasswordRequestDto request)
        {
            var email = request.Email.Trim();

            var genericMessage = "A verification code has been sent.";

            var user = await userManager.FindByEmailAsync(email);

            // Do not reveal whether the email exists or not.
            if (user is null)
            {
                throw new BadRequestException(
                    "User Not Found", ErrorCodes.EmailSendingFailed);

            }
            if (!user.IsActive || user.IsDeleted)
                throw new UnauthorizedException(
                    "This account is inactive.",
                    ErrorCodes.AuthAccountInactive
                );

            // Invalidate old unused OTPs for this user.
            var oldOtps = await context.PasswordResetOtps
                .Where(x => x.UserId == user.Id && !x.IsUsed)
                .ToListAsync();

            foreach (var oldOtp in oldOtps)
            {
                oldOtp.IsUsed = true;
                oldOtp.UsedAt = DateTime.UtcNow;
            }

            var otp = GenerateSixDigitOtp();

            var passwordResetOtp = new PasswordResetOtp
            {
                UserId = user.Id,
                OtpHash = HashOtp(otp),
                ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                FailedAttempts = 0,
                IsVerified = false,
                IsUsed = false,
                CreatedAt = DateTime.UtcNow
            };

            await context.PasswordResetOtps.AddAsync(passwordResetOtp);
            await context.SaveChangesAsync();
            string userName = request.IsArabic ? user.FullNameAr : user.FullNameEn;
            await emailService.SendPasswordResetOtpAsync(
                user.Email!,
                userName,
                otp,
                request.IsArabic
            );

            return new ForgotPasswordMessageResponseDto
            {
                Message = genericMessage
            };
        }
        public async Task<VerifyResetOtpResponseDto> VerifyResetOtpAsync(VerifyResetOtpRequestDto request)
        {
            var email = request.Email.Trim();
            var otp = request.Otp.Trim();

            var user = await userManager.FindByEmailAsync(email);

            if (user is null || user.IsDeleted)
            {
                throw new BadRequestException(
                   "User Not Found", ErrorCodes.UserNotFound
                );
            }

            if (!user.IsActive)
            {
                throw new UnauthorizedException(
                  "This account is inactive.",
                  ErrorCodes.AuthAccountInactive
              );
            }


            var otpHash = HashOtp(otp);

            var otpRecord = await context.PasswordResetOtps
                .Where(x =>
                    x.UserId == user.Id &&
                    !x.IsUsed &&
                    !x.IsVerified)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();

            if (otpRecord is null)
            {
                throw new BadRequestException(
                    "Invalid or expired verification code.",
                    ErrorCodes.InvalidOtp
                );
            }

            if (otpRecord.ExpiresAt < DateTime.UtcNow)
            {
                otpRecord.IsUsed = true;
                otpRecord.UsedAt = DateTime.UtcNow;
                await context.SaveChangesAsync();

                throw new BadRequestException(
                    "Verification code has expired.",
                    ErrorCodes.OtpExpired
                );
            }

            if (otpRecord.FailedAttempts >= 5)
            {
                otpRecord.IsUsed = true;
                otpRecord.UsedAt = DateTime.UtcNow;
                await context.SaveChangesAsync();

                throw new BadRequestException(
                    "Too many invalid attempts. Please request a new code.",
                    ErrorCodes.OtpTooManyAttempts
                );
            }

            if (otpRecord.OtpHash != otpHash)
            {
                otpRecord.FailedAttempts++;
                await context.SaveChangesAsync();

                throw new BadRequestException(
                    "Invalid verification code.",
                    ErrorCodes.InvalidOtp
                );
            }

            var resetSessionToken = GenerateResetSessionToken();

            otpRecord.IsVerified = true;
            otpRecord.VerifiedAt = DateTime.UtcNow;
            otpRecord.ResetSessionToken = resetSessionToken;

            await context.SaveChangesAsync();

            return new VerifyResetOtpResponseDto
            {
                ResetSessionToken = resetSessionToken,
                Message = "Verification code confirmed successfully."
            };
        }
        public async Task<ForgotPasswordMessageResponseDto> ResetPasswordAsync(ResetPasswordRequestDto request)
        {
            if (request.NewPassword != request.ConfirmNewPassword)
            {
                throw new BadRequestException(
                    "New password and confirmation password do not match.",
                    ErrorCodes.PasswordConfirmationMismatch
                );
            }

            var email = request.Email.Trim();
            var resetSessionToken = request.ResetSessionToken.Trim();

            var user = await userManager.FindByEmailAsync(email);

            if (user is null || !user.IsActive || user.IsDeleted)
            {
                throw new BadRequestException(
                    "Invalid password reset request.",
                    ErrorCodes.InvalidPasswordResetRequest
                );
            }

            var otpRecord = await context.PasswordResetOtps
                .Where(x =>
                    x.UserId == user.Id &&
                    x.IsVerified &&
                    !x.IsUsed &&
                    x.ResetSessionToken == resetSessionToken)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();

            if (otpRecord is null)
            {
                throw new BadRequestException(
                    "Invalid password reset request.",
                    ErrorCodes.InvalidPasswordResetRequest
                );
            }

            if (otpRecord.ExpiresAt < DateTime.UtcNow)
            {
                otpRecord.IsUsed = true;
                otpRecord.UsedAt = DateTime.UtcNow;
                await context.SaveChangesAsync();

                throw new BadRequestException(
                    "Password reset session has expired.",
                    ErrorCodes.PasswordResetSessionExpired
                );
            }

            var identityResetToken = await userManager.GeneratePasswordResetTokenAsync(user);

            var resetResult = await userManager.ResetPasswordAsync(
                user,
                identityResetToken,
                request.NewPassword
            );

            if (!resetResult.Succeeded)
            {
                throw new BadRequestException(
                    "Password reset failed.",
                    ErrorCodes.PasswordResetFailed,
                    resetResult.Errors.Select(x => x.Description).ToList()
                );
            }

            otpRecord.IsUsed = true;
            otpRecord.UsedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();

            return new ForgotPasswordMessageResponseDto
            {
                Message = "Password has been reset successfully."
            };
        }
        //helpers
        private static List<UserRole> ParseRoles(IList<string> roleNames)
        {
            if (roleNames is null || roleNames.Count == 0)
                throw new BadRequestException(
                    "User role is not assigned.",
                    ErrorCodes.UserRoleNotAssigned
                );

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

            return parsedRoles;
        }
        private static string GenerateSixDigitOtp()
        {
            return RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
        }

        private static string HashOtp(string otp)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(otp));
            return Convert.ToBase64String(bytes);
        }

        private static string GenerateResetSessionToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64))
                .Replace("+", "")
                .Replace("/", "")
                .Replace("=", "");
        }
    }
}
