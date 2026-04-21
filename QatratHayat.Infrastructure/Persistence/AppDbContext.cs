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
        public DbSet<NationalRegistry> NationalRegistries { get; set; } = null!;
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
        public DbSet<ScreeningSession> ScreeningSessions { get; set; } = null!;
        public DbSet<Notification> Notifications { get; set; } = null!;
        public DbSet<AuditLog> AuditLogs { get; set; } = null!;
        public DbSet<PasswordResetOtp> PasswordResetOtps { get; set; } = null!;


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
            // PasswordResetOtp
            builder.Entity<PasswordResetOtp>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.HasOne<ApplicationUser>()
                      .WithMany()
                      .HasForeignKey(x => x.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(x => x.UserId);

                entity.HasIndex(x => x.ResetSessionToken)
                      .IsUnique()
                      .HasFilter("[ResetSessionToken] IS NOT NULL");

                entity.Property(x => x.OtpHash)
                      .HasMaxLength(256)
                      .IsRequired();

                entity.Property(x => x.ResetSessionToken)
                      .HasMaxLength(128);
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

                // N:1 A deferral record may be linked to one screening session
                entity.HasOne(x => x.ScreeningSession)
                      .WithMany()
                      .HasForeignKey(x => x.ScreeningSessionId)
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

            //ScreeningSession
            builder.Entity<ScreeningSession>(entity =>
            {
                entity.HasKey(x => x.Id);

                // 1:N A user can have multiple screening sessions
                entity.HasOne<ApplicationUser>()
                      .WithMany(x => x.ScreeningSessions)
                      .HasForeignKey(x => x.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                // 1:N A donor profile can have multiple screening sessions
                entity.HasOne(x => x.DonorProfile)
                      .WithMany(x => x.ScreeningSessions)
                      .HasForeignKey(x => x.DonorProfileId)
                      .OnDelete(DeleteBehavior.Restrict);

                // 1:N DonationIntent can have multiple screening sessions
                entity.HasOne(x => x.DonationIntent)
                      .WithMany(x => x.ScreeningSessions)
                      .HasForeignKey(x => x.DonationIntentId)
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

                // 1:N A screening session contains many answers
                entity.HasOne(x => x.ScreeningSession)
                      .WithMany(x => x.Answers)
                      .HasForeignKey(x => x.ScreeningSessionId)
                      .OnDelete(DeleteBehavior.Cascade);


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


            //Screening Question Data
            var seedDate = new DateTime(2026, 4, 8, 0, 0, 0, DateTimeKind.Utc);

            builder.Entity<ScreeningQuestion>().HasData(

                // =========================
                // Registration Questions
                // =========================
                new ScreeningQuestion
                {
                    Id = 1,
                    TextAr = "هل سبق لك تبرعت بالدم؟",
                    TextEn = "Have you donated blood before?",
                    SessionType = ScreeningSessionType.Registration,
                    DeferralType = DeferralType.NoDeferral,
                    DecisionMode = ScreeningDecisionMode.Informational,
                    IsForFemaleOnly = false,
                    RequiresAdditionalText = false,
                    RequiresDateValue = false,
                    IsActive = true,
                    DisplayOrder = 1,
                    CreatedAt = seedDate
                },
                new ScreeningQuestion
                {
                    Id = 2,
                    TextAr = "هل حدثت لك ردود فعل نتيجة التبرع مثل الدوخة أو الإغماء أو غيرها؟",
                    TextEn = "Have you ever experienced reactions after blood donation such as dizziness, fainting, or others?",
                    SessionType = ScreeningSessionType.Registration,
                    DeferralType = DeferralType.NoDeferral,
                    DecisionMode = ScreeningDecisionMode.ReviewWhenYes,
                    IsForFemaleOnly = false,
                    RequiresAdditionalText = false,
                    RequiresDateValue = false,
                    IsActive = true,
                    DisplayOrder = 2,
                    CreatedAt = seedDate,
                    AdditionalTextLabelAr = "",
                    AdditionalTextLabelEn = ""
                },
                new ScreeningQuestion
                {
                    Id = 3,
                    TextAr = "هل أنت مصاب بأمراض الدم الوراثية مثل الثلاسيميا أو فقر الدم المنجلي أو فقر الدم المزمن أو غيرها؟",
                    TextEn = "Do you have hereditary blood disorders such as thalassemia, sickle cell disease, chronic anemia, or others?",
                    SessionType = ScreeningSessionType.Registration,
                    DeferralType = DeferralType.NoDeferral,
                    DecisionMode = ScreeningDecisionMode.ReviewWhenYes,
                    IsForFemaleOnly = false,
                    RequiresAdditionalText = true,
                    RequiresDateValue = false,
                    IsActive = true,
                    DisplayOrder = 3,
                    CreatedAt = seedDate,
                    AdditionalTextLabelAr = "اذكر اسم المرض",
                    AdditionalTextLabelEn = "Please specify the condition"
                },
                new ScreeningQuestion
                {
                    Id = 4,
                    TextAr = "هل أنت مصاب بأمراض نزف الدم أو أي نقص خلقي في أحد عوامل التخثر مثل الهيموفيليا أو غيرها؟",
                    TextEn = "Do you have bleeding disorders or congenital clotting factor deficiencies such as hemophilia or others?",
                    SessionType = ScreeningSessionType.Registration,
                    DeferralType = DeferralType.Permanent,
                    DecisionMode = ScreeningDecisionMode.AutoDeferralWhenYes,
                    IsForFemaleOnly = false,
                    RequiresAdditionalText = false,
                    RequiresDateValue = false,
                    IsActive = true,
                    DisplayOrder = 4,
                    CreatedAt = seedDate,
                    AdditionalTextLabelAr = "",
                    AdditionalTextLabelEn = ""
                },
                new ScreeningQuestion
                {
                    Id = 5,
                    TextAr = "هل أنت مصاب بأمراض المناعة الذاتية مثل مرض بهجت أو الذئبة أو حمى البحر الأبيض المتوسط أو الروماتيزم أو الصدفية أو البهاق أو الثعلبة أو غيرها؟",
                    TextEn = "Do you have autoimmune diseases such as Behçet’s disease, lupus, familial Mediterranean fever, rheumatism, psoriasis, vitiligo, alopecia, or others?",
                    SessionType = ScreeningSessionType.Registration,
                    DeferralType = DeferralType.NoDeferral,
                    DecisionMode = ScreeningDecisionMode.ReviewWhenYes,
                    IsForFemaleOnly = false,
                    RequiresAdditionalText = true,
                    RequiresDateValue = false,
                    IsActive = true,
                    DisplayOrder = 5,
                    CreatedAt = seedDate,
                    AdditionalTextLabelAr = "اذكر اسم المرض",
                    AdditionalTextLabelEn = "Please specify the condition"
                },
                new ScreeningQuestion
                {
                    Id = 6,
                    TextAr = "هل أنت مصاب بأمراض الحساسية المزمنة والشديدة؟",
                    TextEn = "Do you have severe or chronic allergic conditions?",
                    SessionType = ScreeningSessionType.Registration,
                    DeferralType = DeferralType.NoDeferral,
                    DecisionMode = ScreeningDecisionMode.ReviewWhenYes,
                    IsForFemaleOnly = false,
                    RequiresAdditionalText = true,
                    RequiresDateValue = false,
                    IsActive = true,
                    DisplayOrder = 6,
                    CreatedAt = seedDate,
                    AdditionalTextLabelAr = "اذكر نوع الحساسية",
                    AdditionalTextLabelEn = "Please specify the allergy"
                },
                new ScreeningQuestion
                {
                    Id = 7,
                    TextAr = "هل تشكو من نوبات إغماء أو تشنج؟",
                    TextEn = "Do you suffer from fainting episodes or seizures?",
                    SessionType = ScreeningSessionType.Registration,
                    DeferralType = DeferralType.NoDeferral,
                    DecisionMode = ScreeningDecisionMode.ReviewWhenYes,
                    IsForFemaleOnly = false,
                    RequiresAdditionalText = false,
                    RequiresDateValue = false,
                    IsActive = true,
                    DisplayOrder = 7,
                    CreatedAt = seedDate
                },
                new ScreeningQuestion
                {
                    Id = 8,
                    TextAr = "هل تسافر كثيرًا إلى الخارج؟",
                    TextEn = "Do you travel abroad frequently?",
                    SessionType = ScreeningSessionType.Registration,
                    DeferralType = DeferralType.NoDeferral,
                    DecisionMode = ScreeningDecisionMode.Informational,
                    IsForFemaleOnly = false,
                    RequiresAdditionalText = false,
                    RequiresDateValue = false,
                    IsActive = true,
                    DisplayOrder = 8,
                    CreatedAt = seedDate
                },

                // =========================
                // Pre-Donation Questions
                // =========================
                new ScreeningQuestion
                {
                    Id = 101,
                    TextAr = "هل صحتك اليوم ليست جيدة؟",
                    TextEn = "Are you feeling unwell today?",
                    SessionType = ScreeningSessionType.PreDonation,
                    DeferralType = DeferralType.NoDeferral,
                    DecisionMode = ScreeningDecisionMode.ReviewWhenYes,
                    IsForFemaleOnly = false,
                    RequiresAdditionalText = false,
                    RequiresDateValue = false,
                    IsActive = true,
                    DisplayOrder = 1,
                    CreatedAt = seedDate
                },
                new ScreeningQuestion
                {
                    Id = 102,
                    TextAr = "هل سبق وأن أُصبت بالملاريا خلال الثلاث سنوات الماضية؟",
                    TextEn = "Have you had malaria during the past 3 years?",
                    SessionType = ScreeningSessionType.PreDonation,
                    DeferralType = DeferralType.Temporary,
                    DecisionMode = ScreeningDecisionMode.AutoDeferralWhenYes,
                    IsForFemaleOnly = false,
                    RequiresAdditionalText = false,
                    RequiresDateValue = true,
                    IsActive = true,
                    DisplayOrder = 2,
                    CreatedAt = seedDate,
                    ConditionalDateLabelAr = "تاريخ الإصابة أو انتهاء العلاج",
                    ConditionalDateLabelEn = "Date of illness or completion of treatment",
                    DeferralPeriodDays = 1095
                },
                new ScreeningQuestion
                {
                    Id = 103,
                    TextAr = "هل سافرت مؤخرًا إلى منطقة يتفشى فيها مرض الملاريا؟",
                    TextEn = "Have you recently traveled to an area where malaria is endemic?",
                    SessionType = ScreeningSessionType.PreDonation,
                    DeferralType = DeferralType.Temporary,
                    DecisionMode = ScreeningDecisionMode.AutoDeferralWhenYes,
                    IsForFemaleOnly = false,
                    RequiresAdditionalText = false,
                    RequiresDateValue = true,
                    IsActive = true,
                    DisplayOrder = 3,
                    CreatedAt = seedDate,
                    ConditionalDateLabelAr = "تاريخ العودة من السفر",
                    ConditionalDateLabelEn = "Date of return from travel",
                    DeferralPeriodDays = 90
                },
                new ScreeningQuestion
                {
                    Id = 104,
                    TextAr = "هل سبق وأن أُصبت بالحمى المالطية أو حمى التيفوئيد؟",
                    TextEn = "Have you ever had brucellosis or typhoid fever?",
                    SessionType = ScreeningSessionType.PreDonation,
                    DeferralType = DeferralType.NoDeferral,
                    DecisionMode = ScreeningDecisionMode.ReviewWhenYes,
                    IsForFemaleOnly = false,
                    RequiresAdditionalText = false,
                    RequiresDateValue = false,
                    IsActive = true,
                    DisplayOrder = 4,
                    CreatedAt = seedDate
                },
                new ScreeningQuestion
                {
                    Id = 105,
                    TextAr = "هل سبق وأن أُصبت باليرقان؟",
                    TextEn = "Have you ever had jaundice?",
                    SessionType = ScreeningSessionType.PreDonation,
                    DeferralType = DeferralType.NoDeferral,
                    DecisionMode = ScreeningDecisionMode.ReviewWhenYes,
                    IsForFemaleOnly = false,
                    RequiresAdditionalText = false,
                    RequiresDateValue = false,
                    IsActive = true,
                    DisplayOrder = 5,
                    CreatedAt = seedDate
                },
                new ScreeningQuestion
                {
                    Id = 106,
                    TextAr = "هل سبق وأن أُصبت بالتهاب الكبد الفيروسي؟",
                    TextEn = "Have you ever had viral hepatitis?",
                    SessionType = ScreeningSessionType.PreDonation,
                    DeferralType = DeferralType.NoDeferral,
                    DecisionMode = ScreeningDecisionMode.ReviewWhenYes,
                    IsForFemaleOnly = false,
                    RequiresAdditionalText = false,
                    RequiresDateValue = false,
                    IsActive = true,
                    DisplayOrder = 6,
                    CreatedAt = seedDate
                },
                new ScreeningQuestion
                {
                    Id = 107,
                    TextAr = "هل خالطت شخصًا مصابًا بالتهاب الكبد الفيروسي خلال الاثني عشر شهرًا الماضية؟",
                    TextEn = "Have you had close contact with someone with viral hepatitis during the past 12 months?",
                    SessionType = ScreeningSessionType.PreDonation,
                    DeferralType = DeferralType.Temporary,
                    DecisionMode = ScreeningDecisionMode.AutoDeferralWhenYes,
                    IsForFemaleOnly = false,
                    RequiresAdditionalText = false,
                    RequiresDateValue = true,
                    IsActive = true,
                    DisplayOrder = 7,
                    CreatedAt = seedDate,
                    ConditionalDateLabelAr = "تاريخ آخر مخالطة",
                    ConditionalDateLabelEn = "Date of last contact",
                    DeferralPeriodDays = 365
                },
                new ScreeningQuestion
                {
                    Id = 108,
                    TextAr = "هل أنت مصاب بأحد الأمراض المعدية التي تنتقل عن طريق الدم مثل الإيدز أو السفلس؟",
                    TextEn = "Do you have any blood-borne infectious diseases such as HIV/AIDS or syphilis?",
                    SessionType = ScreeningSessionType.PreDonation,
                    DeferralType = DeferralType.Permanent,
                    DecisionMode = ScreeningDecisionMode.AutoDeferralWhenYes,
                    IsForFemaleOnly = false,
                    RequiresAdditionalText = false,
                    RequiresDateValue = false,
                    IsActive = true,
                    DisplayOrder = 8,
                    CreatedAt = seedDate
                },
                new ScreeningQuestion
                {
                    Id = 109,
                    TextAr = "هل أنت مصاب بأي من أمراض الرئة أو القلب أو الكلى أو السكري المعتمد على الإنسولين أو ارتفاع ضغط الدم؟",
                    TextEn = "Do you have any of the following conditions: lung disease, heart disease, kidney disease, insulin-dependent diabetes, or high blood pressure?",
                    SessionType = ScreeningSessionType.PreDonation,
                    DeferralType = DeferralType.NoDeferral,
                    DecisionMode = ScreeningDecisionMode.ReviewWhenYes,
                    IsForFemaleOnly = false,
                    RequiresAdditionalText = true,
                    RequiresDateValue = false,
                    IsActive = true,
                    DisplayOrder = 9,
                    CreatedAt = seedDate,
                    AdditionalTextLabelAr = "اذكر الحالة المرضية",
                    AdditionalTextLabelEn = "Please specify the condition"
                },
                new ScreeningQuestion
                {
                    Id = 110,
                    TextAr = "هل تتناول أدوية لأي مرض؟",
                    TextEn = "Are you taking any medications for any illness?",
                    SessionType = ScreeningSessionType.PreDonation,
                    DeferralType = DeferralType.NoDeferral,
                    DecisionMode = ScreeningDecisionMode.ReviewWhenYes,
                    IsForFemaleOnly = false,
                    RequiresAdditionalText = true,
                    RequiresDateValue = false,
                    IsActive = true,
                    DisplayOrder = 10,
                    CreatedAt = seedDate,
                    AdditionalTextLabelAr = "اذكر اسم الدواء",
                    AdditionalTextLabelEn = "Please specify the medication"
                },
                new ScreeningQuestion
                {
                    Id = 111,
                    TextAr = "هل تناولت أسبرين أو أي دواء يحتوي على مكونات الأسبرين خلال الثلاثة أيام الماضية؟",
                    TextEn = "Have you taken aspirin or any medication containing aspirin during the past 3 days?",
                    SessionType = ScreeningSessionType.PreDonation,
                    DeferralType = DeferralType.NoDeferral,
                    DecisionMode = ScreeningDecisionMode.ReviewWhenYes,
                    IsForFemaleOnly = false,
                    RequiresAdditionalText = false,
                    RequiresDateValue = true,
                    IsActive = true,
                    DisplayOrder = 11,
                    CreatedAt = seedDate,
                    ConditionalDateLabelAr = "تاريخ آخر جرعة",
                    ConditionalDateLabelEn = "Date of last dose"
                },
                new ScreeningQuestion
                {
                    Id = 112,
                    TextAr = "هل تناولت أي مسكنات للألم أو مضادات للالتهاب خلال الثلاثة أيام الماضية؟",
                    TextEn = "Have you taken any painkillers or anti-inflammatory medications during the past 3 days?",
                    SessionType = ScreeningSessionType.PreDonation,
                    DeferralType = DeferralType.NoDeferral,
                    DecisionMode = ScreeningDecisionMode.ReviewWhenYes,
                    IsForFemaleOnly = false,
                    RequiresAdditionalText = true,
                    RequiresDateValue = true,
                    IsActive = true,
                    DisplayOrder = 12,
                    CreatedAt = seedDate,
                    ConditionalDateLabelAr = "تاريخ آخر جرعة",
                    ConditionalDateLabelEn = "Date of last dose",
                    AdditionalTextLabelAr = "اذكر اسم الدواء",
                    AdditionalTextLabelEn = "Please specify the medication"
                },
                new ScreeningQuestion
                {
                    Id = 113,
                    TextAr = "هل خلعت سنًا أو أجريت علاجًا للأسنان خلال السبعة أيام الماضية؟",
                    TextEn = "Have you had a tooth extraction or dental treatment during the past 7 days?",
                    SessionType = ScreeningSessionType.PreDonation,
                    DeferralType = DeferralType.NoDeferral,
                    DecisionMode = ScreeningDecisionMode.ReviewWhenYes,
                    IsForFemaleOnly = false,
                    RequiresAdditionalText = false,
                    RequiresDateValue = true,
                    IsActive = true,
                    DisplayOrder = 13,
                    CreatedAt = seedDate,
                    ConditionalDateLabelAr = "تاريخ الإجراء",
                    ConditionalDateLabelEn = "Date of the procedure"
                },
                new ScreeningQuestion
                {
                    Id = 114,
                    TextAr = "هل تعرضت لثقب بالأذن أو الجلد أو للعلاج بالإبر الصينية خلال الاثني عشر شهرًا الماضية؟",
                    TextEn = "Have you had ear/body piercing or acupuncture during the past 12 months?",
                    SessionType = ScreeningSessionType.PreDonation,
                    DeferralType = DeferralType.Temporary,
                    DecisionMode = ScreeningDecisionMode.AutoDeferralWhenYes,
                    IsForFemaleOnly = false,
                    RequiresAdditionalText = false,
                    RequiresDateValue = true,
                    IsActive = true,
                    DisplayOrder = 14,
                    CreatedAt = seedDate,
                    ConditionalDateLabelAr = "تاريخ الإجراء",
                    ConditionalDateLabelEn = "Date of the procedure",
                    DeferralPeriodDays = 365
                },
                new ScreeningQuestion
                {
                    Id = 115,
                    TextAr = "هل أُجريت لك عملية جراحية خلال الاثني عشر شهرًا الماضية؟",
                    TextEn = "Have you had surgery during the past 12 months?",
                    SessionType = ScreeningSessionType.PreDonation,
                    DeferralType = DeferralType.NoDeferral,
                    DecisionMode = ScreeningDecisionMode.ReviewWhenYes,
                    IsForFemaleOnly = false,
                    RequiresAdditionalText = true,
                    RequiresDateValue = true,
                    IsActive = true,
                    DisplayOrder = 15,
                    CreatedAt = seedDate,
                    ConditionalDateLabelAr = "تاريخ العملية",
                    ConditionalDateLabelEn = "Date of surgery",
                    AdditionalTextLabelAr = "اذكر نوع العملية",
                    AdditionalTextLabelEn = "Please specify the type of surgery"
                },
                new ScreeningQuestion
                {
                    Id = 116,
                    TextAr = "هل تلقيت نقل دم أو مشتقاته خلال الاثني عشر شهرًا الماضية؟",
                    TextEn = "Have you received a blood transfusion or blood products during the past 12 months?",
                    SessionType = ScreeningSessionType.PreDonation,
                    DeferralType = DeferralType.Temporary,
                    DecisionMode = ScreeningDecisionMode.AutoDeferralWhenYes,
                    IsForFemaleOnly = false,
                    RequiresAdditionalText = false,
                    RequiresDateValue = true,
                    IsActive = true,
                    DisplayOrder = 16,
                    CreatedAt = seedDate,
                    ConditionalDateLabelAr = "تاريخ نقل الدم",
                    ConditionalDateLabelEn = "Date of transfusion",
                    DeferralPeriodDays = 365
                },
                new ScreeningQuestion
                {
                    Id = 117,
                    TextAr = "هل عملت حجامة خلال الثمانية أشهر الماضية؟",
                    TextEn = "Have you had cupping therapy during the past 8 months?",
                    SessionType = ScreeningSessionType.PreDonation,
                    DeferralType = DeferralType.NoDeferral,
                    DecisionMode = ScreeningDecisionMode.ReviewWhenYes,
                    IsForFemaleOnly = false,
                    RequiresAdditionalText = false,
                    RequiresDateValue = true,
                    IsActive = true,
                    DisplayOrder = 17,
                    CreatedAt = seedDate,
                    ConditionalDateLabelAr = "تاريخ الحجامة",
                    ConditionalDateLabelEn = "Date of cupping"
                },
                new ScreeningQuestion
                {
                    Id = 118,
                    TextAr = "هل عملت وشمًا خلال الاثني عشر شهرًا الماضية؟",
                    TextEn = "Have you had a tattoo during the past 12 months?",
                    SessionType = ScreeningSessionType.PreDonation,
                    DeferralType = DeferralType.Temporary,
                    DecisionMode = ScreeningDecisionMode.AutoDeferralWhenYes,
                    IsForFemaleOnly = false,
                    RequiresAdditionalText = false,
                    RequiresDateValue = true,
                    IsActive = true,
                    DisplayOrder = 18,
                    CreatedAt = seedDate,
                    ConditionalDateLabelAr = "تاريخ الوشم",
                    ConditionalDateLabelEn = "Date of tattoo",
                    DeferralPeriodDays = 365
                },
                new ScreeningQuestion
                {
                    Id = 119,
                    TextAr = "هل استخدمت إبرًا لأخذ أي نوع من المخدرات عن طريق الوريد أو تحت الجلد؟",
                    TextEn = "Have you ever used needles to take non-prescribed drugs intravenously or under the skin?",
                    SessionType = ScreeningSessionType.PreDonation,
                    DeferralType = DeferralType.Permanent,
                    DecisionMode = ScreeningDecisionMode.AutoDeferralWhenYes,
                    IsForFemaleOnly = false,
                    RequiresAdditionalText = false,
                    RequiresDateValue = false,
                    IsActive = true,
                    DisplayOrder = 19,
                    CreatedAt = seedDate
                },
                new ScreeningQuestion
                {
                    Id = 120,
                    TextAr = "هل أخذت أي مطعوم خلال الأربعة أسابيع الماضية؟",
                    TextEn = "Have you received any vaccine during the past 4 weeks?",
                    SessionType = ScreeningSessionType.PreDonation,
                    DeferralType = DeferralType.NoDeferral,
                    DecisionMode = ScreeningDecisionMode.ReviewWhenYes,
                    IsForFemaleOnly = false,
                    RequiresAdditionalText = true,
                    RequiresDateValue = true,
                    IsActive = true,
                    DisplayOrder = 20,
                    CreatedAt = seedDate,
                    ConditionalDateLabelAr = "تاريخ المطعوم",
                    ConditionalDateLabelEn = "Date of vaccination",
                    AdditionalTextLabelAr = "اذكر اسم المطعوم",
                    AdditionalTextLabelEn = "Please specify the vaccine"
                },
                new ScreeningQuestion
                {
                    Id = 121,
                    TextAr = "للنساء: هل أنتِ حامل الآن؟",
                    TextEn = "For women: Are you currently pregnant?",
                    SessionType = ScreeningSessionType.PreDonation,
                    DeferralType = DeferralType.Temporary,
                    DecisionMode = ScreeningDecisionMode.AutoDeferralWhenYes,
                    IsForFemaleOnly = true,
                    RequiresAdditionalText = false,
                    RequiresDateValue = false,
                    IsActive = true,
                    DisplayOrder = 21,
                    CreatedAt = seedDate
                },
                new ScreeningQuestion
                {
                    Id = 122,
                    TextAr = "للنساء: هل حدث حمل أو إجهاض خلال الأشهر التسعة الماضية؟",
                    TextEn = "For women: Have you been pregnant or had a miscarriage during the past 9 months?",
                    SessionType = ScreeningSessionType.PreDonation,
                    DeferralType = DeferralType.Temporary,
                    DecisionMode = ScreeningDecisionMode.AutoDeferralWhenYes,
                    IsForFemaleOnly = true,
                    RequiresAdditionalText = false,
                    RequiresDateValue = true,
                    IsActive = true,
                    DisplayOrder = 22,
                    CreatedAt = seedDate,
                    ConditionalDateLabelAr = "تاريخ آخر حمل أو إجهاض",
                    ConditionalDateLabelEn = "Date of last pregnancy or miscarriage",
                    DeferralPeriodDays = 270
                },
                new ScreeningQuestion
                {
                    Id = 123,
                    TextAr = "للنساء: هل انتهت الرضاعة الطبيعية قبل أقل من ثلاثة أشهر؟",
                    TextEn = "For women: Did breastfeeding end less than 3 months ago?",
                    SessionType = ScreeningSessionType.PreDonation,
                    DeferralType = DeferralType.NoDeferral,
                    DecisionMode = ScreeningDecisionMode.ReviewWhenYes,
                    IsForFemaleOnly = true,
                    RequiresAdditionalText = false,
                    RequiresDateValue = true,
                    IsActive = true,
                    DisplayOrder = 23,
                    CreatedAt = seedDate,
                    ConditionalDateLabelAr = "تاريخ انتهاء الرضاعة الطبيعية",
                    ConditionalDateLabelEn = "Date breastfeeding ended"
                },
                new ScreeningQuestion
                {
                    Id = 124,
                    TextAr = "للنساء: هل تعانين الآن من الدورة الشهرية مع زيادة في كمية الدم المفقودة؟",
                    TextEn = "For women: Are you currently menstruating with increased blood loss?",
                    SessionType = ScreeningSessionType.PreDonation,
                    DeferralType = DeferralType.NoDeferral,
                    DecisionMode = ScreeningDecisionMode.ReviewWhenYes,
                    IsForFemaleOnly = true,
                    RequiresAdditionalText = false,
                    RequiresDateValue = false,
                    IsActive = true,
                    DisplayOrder = 24,
                    CreatedAt = seedDate
                }
            );

        }
    }
}