using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QatratHayat.Application.Common.Interfaces;
using QatratHayat.Application.Common.Settings;
using QatratHayat.Application.Features.Auth.Interfaces;
using QatratHayat.Application.Features.BloodRequests.Interfaces;
using QatratHayat.Application.Features.BloodRequests.Services;
using QatratHayat.Application.Features.BranchManagement.Interfaces;
using QatratHayat.Application.Features.BranchManagement.Services;
using QatratHayat.Application.Features.Donations.Interfaces;
using QatratHayat.Application.Features.HospitalManagement.Interfaces;
using QatratHayat.Application.Features.HospitalManagement.Services;
using QatratHayat.Application.Features.Inventory.Interfaces;
using QatratHayat.Application.Features.ScreeningQuestions.Interfaces;
using QatratHayat.Application.Features.UsersManagement.Interfaces;
using QatratHayat.Application.Features.UsersManagement.Services;
using QatratHayat.Infrastructure.BackgroundJobs;
using QatratHayat.Infrastructure.Identity;
using QatratHayat.Infrastructure.Persistence;
using QatratHayat.Infrastructure.Services;

namespace QatratHayat.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
            );

            services
                .AddIdentityCore<ApplicationUser>(options =>
                {
                    options.Password.RequiredLength = 8;
                    options.Password.RequireDigit = true;
                    options.Password.RequireUppercase = true;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireNonAlphanumeric = false;

                    options.User.RequireUniqueEmail = true;
                })
                .AddRoles<ApplicationRole>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();
            services.AddHttpContextAccessor();

            services.AddScoped<IJwtTokenService, JwtTokenService>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<ICivilStatusService, CivilStatusService>();
            services.AddScoped<IScreeningSessionService, ScreeningSessionService>();
            services.AddScoped<IUsersManagementService, UsersManagementService>();
            services.AddScoped<IBranchManagementService, BranchManagementService>();
            services.AddScoped<IHospitalManagementService, HospitalManagementService>();
            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IBloodRequestService, BloodRequestService>();
            services.AddScoped<IBloodTypeCompatibilityService, BloodTypeCompatibilityService>();
            services.AddScoped<IDonationService, DonationService>();
            services.AddScoped<IUnitCodeGenerator, UnitCodeGenerator>();
            services.AddScoped<IBloodUnitSmartAllocationService, BloodUnitSmartAllocationService>();
            services.AddHostedService<DonationIntentExpirationBackgroundService>();
            services.AddHostedService<BloodUnitExpirationBackgroundService>();
            services.AddScoped<IInventoryService, InventoryService>();
            return services;
        }
    }
}
