using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using QatratHayat.Domain.Entities;
using QatratHayat.Domain.Enums;
using QatratHayat.Infrastructure.Identity;

namespace QatratHayat.Infrastructure.Persistence
{
    public class AppDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, int>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }
        
        //DBSet here
        public DbSet<NationalRegistry> NationalRegistries { get; set; }= null!;
        public DbSet<DonorProfile> DonorProfiles { get; set; } = null!;
        public DbSet<Donation> Donations { get; set; } = null!;
        public DbSet<Beneficiary> Beneficiaries { get; set; } = null!;
        public DbSet<BloodRequest> BloodRequests { get; set; } = null!;
        public DbSet<DeferralRecord> DeferralRecords { get; set; } = null!;
        public DbSet<DonationIntent> DonationIntents { get; set; } = null!;
        public DbSet<Branch> Branches { get; set; } = null!;
        public DbSet<BranchWorkingHour> BranchWorkingHours { get; set; } = null!;
        public DbSet<Hospital> Hospitals { get; set; } = null!;
        public DbSet<Campaign> Campaigns { get; set; } = null!;
        public DbSet<CampaignTargetBloodType> CampaignTargetBloodTypes { get; set; } = null!;
        public DbSet<BloodUnit> BloodUnits { get; set; } = null!;
        public DbSet<ScreeningQuestion> ScreeningQuestions { get; set; } = null!;
        public DbSet<ScreeningAnswer> ScreeningAnswers { get; set; } = null!;
        public DbSet<Notification> Notifications { get; set; } = null!;
        public DbSet<AuditLog> AuditLogs { get; set; } = null!;
        

        //Give constrains for Tables in DB 
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // NationalRegistry
            builder.Entity<NationalRegistry>
                (
                    entity =>
                    {
                        entity.HasKey(x => x.Id);

                        entity.HasIndex(x => x.NationalId)
                            .IsUnique();

                    }

                );

            // ApplicationRole
            builder.Entity<ApplicationRole>().HasData(
                new ApplicationRole { Id = 1, Name = UserRole.Citizen.ToString(), NormalizedName = UserRole.Citizen.ToString().ToUpper() },
                new ApplicationRole { Id = 2, Name = UserRole.Doctor.ToString(), NormalizedName = UserRole.Doctor.ToString().ToUpper() },
                new ApplicationRole { Id = 3, Name = UserRole.Employee.ToString(), NormalizedName = UserRole.Employee.ToString().ToUpper() },
                new ApplicationRole { Id = 4, Name = UserRole.BranchManager.ToString(), NormalizedName = UserRole.BranchManager.ToString().ToUpper() },
                new ApplicationRole { Id = 5, Name = UserRole.Admin.ToString(), NormalizedName = UserRole.Admin.ToString().ToUpper() }
            );

            // ApplicationUser
            builder.Entity<ApplicationUser>(entity =>
            {

                entity.HasIndex(x => x.NationalId)
                    .IsUnique();
                // N:1 Employee/Manager belongs to one branch
                entity.HasOne(x => x.Branch)
                    .WithMany()
                    .HasForeignKey(x => x.BranchId)
                    .OnDelete(DeleteBehavior.Restrict);

                // N:1 Doctors belongs to one hospital
                entity.HasOne(x => x.Hospital)
                    .WithMany()
                    .HasForeignKey(x => x.HospitalId)
                    .OnDelete(DeleteBehavior.Restrict);

                // 1:N A user receives many notifications
                entity.HasMany(x => x.Notifications)
                      .WithOne()
                      .HasForeignKey(x => x.RecipientUserId)
                      .OnDelete(DeleteBehavior.Restrict);

            });

            // DonorProfile
            builder.Entity<DonorProfile>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.HasIndex(x => x.UserId)
                    .IsUnique();

                // 1:1 Each citizen user has exactly one donor profile
                entity.HasOne<ApplicationUser>()
                    .WithOne(x => x.DonorProfile)
                    .HasForeignKey<DonorProfile>(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // N:1 A donor profile blood type can be confirmed by one employee
                entity.HasOne<ApplicationUser>()
                    .WithMany()
                    .HasForeignKey(x => x.BloodTypeConfirmedByEmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);

            });

            // DeferralRecord
            builder.Entity<DeferralRecord>(entity =>
            {
                entity.HasKey(x => x.Id);

                // 1:N A donor can have multiple deferral history entries
                entity.HasOne(x => x.DonorProfile)
                    .WithMany(x => x.DeferralRecords)
                    .HasForeignKey(x => x.DonorProfileId)
                    .OnDelete(DeleteBehavior.Cascade);

                // N:1 A deferral record may be linked to one screening question
                entity.HasOne(x => x.ScreeningQuestion)
                      .WithMany()
                      .HasForeignKey(x => x.ScreeningQuestionId)
                      .OnDelete(DeleteBehavior.Restrict);

                // N:1 A deferral decision may be made by one user
                entity.HasOne<ApplicationUser>()
                      .WithMany()
                      .HasForeignKey(x => x.DecidedByUserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Donation
            builder.Entity<Donation>(entity =>
            {
                entity.HasKey(x => x.Id);

                // 1:N A donor can have multiple completed donations
                entity.HasOne(x => x.DonorProfile)
                     .WithMany(x => x.Donations)
                     .HasForeignKey(x => x.DonorProfileId)
                     .OnDelete(DeleteBehavior.Restrict);

                // 1:N A request may receive multiple donations
                entity.HasOne(x => x.BloodRequest)
                      .WithMany(x => x.Donations)
                      .HasForeignKey(x => x.BloodRequestId)
                      .OnDelete(DeleteBehavior.Restrict);

                // 1:N A campaign can have many linked donations
                entity.HasOne(x => x.Campaign)
                      .WithMany(x => x.Donations)
                      .HasForeignKey(x => x.CampaignId)
                      .OnDelete(DeleteBehavior.Restrict);

                // 1:N A branch can process many donations
                entity.HasOne(x => x.Branch)
                      .WithMany(x => x.Donations)
                      .HasForeignKey(x => x.BranchId)
                      .OnDelete(DeleteBehavior.Restrict);

                // 1:N An employee can process many donations
                entity.HasOne<ApplicationUser>()
                      .WithMany(x => x.ProcessedDonations)
                      .HasForeignKey(x => x.EmployeeUserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // DonationIntent
            builder.Entity<DonationIntent>(entity =>
            {
                entity.HasKey(x => x.Id);

                // 1:N A donor can have multiple intents over time
                entity.HasOne(x => x.DonorProfile)
                     .WithMany(x => x.DonationIntents)
                     .HasForeignKey(x => x.DonorProfileId)
                     .OnDelete(DeleteBehavior.Restrict);

                // N:1 An intent may target a campaign (DonationType=Campaign)
                entity.HasOne(x => x.Campaign)
                       .WithMany(x => x.DonationIntents)
                       .HasForeignKey(x => x.CampaignId)
                       .OnDelete(DeleteBehavior.Restrict);

                // N:1 An intent may target a request (DonationType=Request)
                entity.HasOne(x => x.BloodRequest)
                       .WithMany(x => x.DonationIntents)
                       .HasForeignKey(x => x.BloodRequestId)
                       .OnDelete(DeleteBehavior.Restrict);
            });

            // BloodRequest
            builder.Entity<BloodRequest>(entity =>
            {
                entity.HasKey(x => x.Id);

                // N:1 A request is for one beneficiary; a beneficiary can have multiple requests
                entity.HasOne(x => x.Beneficiary)
                       .WithMany(x => x.BloodRequests)
                       .HasForeignKey(x => x.BeneficiaryId)
                       .OnDelete(DeleteBehavior.Restrict);

                // 1:N A requester user can create many blood requests
                entity.HasOne<ApplicationUser>()
                      .WithMany(x => x.CreatedBloodRequests)
                      .HasForeignKey(x => x.RequesterUserId)
                      .OnDelete(DeleteBehavior.Restrict);

                // 1:N A hospital can have many blood requests
                entity.HasOne(x => x.Hospital)
                      .WithMany(x => x.BloodRequests)
                      .HasForeignKey(x => x.HospitalId)
                      .OnDelete(DeleteBehavior.Restrict);

                // N:1 A request is approved/owned by one doctor
                entity.HasOne<ApplicationUser>()
                      .WithMany()
                      .HasForeignKey(x => x.DoctorId)
                      .OnDelete(DeleteBehavior.Restrict);

                // N:1 A request may be cancelled by one user
                entity.HasOne<ApplicationUser>()
                      .WithMany()
                      .HasForeignKey(x => x.CancelledByUserId)
                      .OnDelete(DeleteBehavior.Restrict);

            });

            // Beneficiary
            builder.Entity<Beneficiary>(entity =>
            {
                entity.HasKey(x => x.Id);

                // N:1 A beneficiary may be linked to one registered user
                entity.HasOne<ApplicationUser>()
                      .WithMany()
                      .HasForeignKey(x => x.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                // N:1 A beneficiary may be merged into one registered user
                entity.HasOne<ApplicationUser>()
                      .WithMany()
                      .HasForeignKey(x => x.MergedIntoUserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // BloodUnit
            builder.Entity<BloodUnit>(entity =>
            {
                entity.HasKey(x => x.Id);

                // 1:N A request may have multiple units allocated
                entity.HasOne(x => x.BloodRequest)
                      .WithMany(x => x.BloodUnits)
                      .HasForeignKey(x => x.AllocatedToRequestId)
                      .OnDelete(DeleteBehavior.Restrict);
                // 1:1 Each donation creates exactly one blood unit
                entity.HasOne(x => x.Donation)
                      .WithOne(x => x.BloodUnit)
                      .HasForeignKey<BloodUnit>(x => x.DonationId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.DonationId)
                      .IsUnique();

                // 1:N A branch can store many blood units
                entity.HasOne(x => x.Branch)
                      .WithMany(x => x.BloodUnits)
                      .HasForeignKey(x => x.BranchId)
                      .OnDelete(DeleteBehavior.Restrict);

                // N:1 A blood unit may be disposed by one employee
                entity.HasOne<ApplicationUser>()
                      .WithMany()
                      .HasForeignKey(x => x.DisposedByEmployeeId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Campaign
            builder.Entity<Campaign>(entity =>
            {
                entity.HasKey(x => x.Id);

                // 1:N A user can create many campaigns
                entity.HasOne<ApplicationUser>()
                      .WithMany(x => x.CreatedCampaigns)
                      .HasForeignKey(x => x.CreatedByUserId)
                      .OnDelete(DeleteBehavior.Restrict);

                // N:1 A campaign may belong to one branch
                entity.HasOne(x => x.Branch)
                      .WithMany(x => x.Campaigns)
                      .HasForeignKey(x => x.BranchId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // CampaignTargetBloodType
            builder.Entity<CampaignTargetBloodType>(entity =>
            {
                entity.HasKey(x => x.Id);

                // 1:N A campaign can target multiple blood types
                entity.HasOne(x => x.Campaign)
                      .WithMany(x => x.TargetBloodTypes)
                      .HasForeignKey(x => x.CampaignId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Branch
            builder.Entity<Branch>(entity =>
            {
                entity.HasKey(x => x.Id);

                // 1:1 Each branch has exactly one manager
                entity.HasOne<ApplicationUser>()
                      .WithMany()
                      .HasForeignKey(x => x.ManagerUserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.ManagerUserId)
                      .IsUnique();
            });
            // BranchWorkingHour
            builder.Entity<BranchWorkingHour>(entity =>
            {
                entity.HasKey(x => x.Id);

            });

            // Hospital
            builder.Entity<Hospital>(entity =>
            {
                entity.HasKey(x => x.Id);

                // N:1 A hospital belongs to one branch
                entity.HasOne(x => x.Branch)
                      .WithMany(x => x.Hospitals)
                      .HasForeignKey(x => x.BranchId)
                      .OnDelete(DeleteBehavior.Restrict);

            });

            // ScreeningQuestion
            builder.Entity<ScreeningQuestion>(entity =>
            {
                entity.HasKey(x => x.Id);

            });

            // ScreeningAnswer
            builder.Entity<ScreeningAnswer>(entity =>
            {
                entity.HasKey(x => x.Id);

                // 1:N A user has answers across multiple screening sessions (registration + pre-donation)
                entity.HasOne<ApplicationUser>()
                    .WithMany(x => x.ScreeningAnswers)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                // 1:N A donor profile links to pre-donation answers (nullable for registration answers)
                entity.HasOne(x => x.DonorProfile)
                     .WithMany(x => x.ScreeningAnswers)
                     .HasForeignKey(x => x.DonorProfileId)
                     .OnDelete(DeleteBehavior.Restrict);

                // 1:N A question has many answers across donors
                entity.HasOne(x => x.ScreeningQuestion)
                      .WithMany(x => x.ScreeningAnswers)
                      .HasForeignKey(x => x.ScreeningQuestionId)
                      .OnDelete(DeleteBehavior.Restrict);

                // N:1 A screening answer may belong to one donation intent
                entity.HasOne(x => x.DonationIntent)
                      .WithMany(x => x.ScreeningAnswers)
                      .HasForeignKey(x => x.DonationIntentId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Notification
            builder.Entity<Notification>(entity =>
            {
                entity.HasKey(x => x.Id);

            });

            // AuditLog
            builder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(x => x.Id);

                // N:1 An audit log entry is created by one user
                entity.HasOne<ApplicationUser>()
                      .WithMany()
                      .HasForeignKey(x => x.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

        }
    }
}