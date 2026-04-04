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
        public DbSet<DonorProfile> donorProfiles { get; set; } = null!;
        public DbSet<Branch> Branches { get; set; } = null!;
        public DbSet<Hospital> Hospitals { get; set; } = null!;
        public DbSet<Beneficiary>BloodRequesters  { get; set; } = null!;
        public DbSet<Campaign> Campaigns { get; set; } = null!;
        public DbSet<CampaignTargetBloodType> CampaignTargetBloodTypes { get; set; } = null!;


        //Give constrains for Tables in DB 
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(x => x.NationalId)
                    .HasMaxLength(10)
                    .IsRequired();

                entity.Property(x => x.FullNameAr)
                    .HasMaxLength(256)
                    .IsRequired();

                entity.Property(x => x.FullNameEn)
                    .HasMaxLength(256)
                    .IsRequired();

                entity.Property(x => x.DateOfBirth)
                    .IsRequired();
                entity.Property(x=>x.Gender)
                .IsRequired();

                entity.Property(x => x.CreatedAt)
                    .IsRequired();

                entity.HasIndex(x => x.NationalId)
                    .IsUnique();
            });
            builder.Entity<ApplicationRole>().HasData(
                new ApplicationRole { Id = 1, Name = UserRole.Citizen.ToString(), NormalizedName = UserRole.Citizen.ToString().ToUpper() },
                new ApplicationRole { Id = 2, Name = UserRole.Doctor.ToString(), NormalizedName = UserRole.Doctor.ToString().ToUpper() },
                new ApplicationRole { Id = 3, Name = UserRole.Employee.ToString(), NormalizedName = UserRole.Employee.ToString().ToUpper() },
                new ApplicationRole { Id = 4, Name = UserRole.BranchManager.ToString(), NormalizedName = UserRole.BranchManager.ToString().ToUpper() },
                new ApplicationRole { Id = 5, Name = UserRole.Admin.ToString(), NormalizedName = UserRole.Admin.ToString().ToUpper() }
            );
            builder.Entity<NationalRegistry>
                (
                    entity =>
                    {
                        entity.HasKey(x => x.Id);

                        entity.Property(x => x.NationalId)
                        .IsRequired()
                        .HasMaxLength(10);

                        entity.Property(x => x.FullNameAr)
                        .HasMaxLength(256)
                        .IsRequired();

                        entity.Property(x => x.FullNameEn)
                        .HasMaxLength(256)
                        .IsRequired();

                        entity.Property(x => x.DateOfBirth)
                            .IsRequired();

                        entity.Property(x => x.Gender)
                            .HasMaxLength(20)
                            .IsRequired();

                        entity.Property(x => x.BloodType)
                            .HasMaxLength(10)
                            .IsRequired();

                        entity.Property(x => x.IsJordanian)
                            .IsRequired();

                        entity.HasIndex(x => x.NationalId)
                            .IsUnique();

                    }

                );
            builder.Entity<DonorProfile>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.UserId)
                    .IsRequired();

                entity.Property(x => x.BloodType)
                    .HasMaxLength(10);

                entity.Property(x => x.BloodTypeStatus)
                    .IsRequired();

                entity.Property(x => x.EligibilityStatus)
                    .IsRequired();

                entity.Property(x => x.PermanentDeferralReason)
                    .HasMaxLength(500);

                entity.Property(x => x.CreatedAt)
                    .IsRequired();

                entity.HasIndex(x => x.UserId)
                    .IsUnique();

                entity.HasOne<ApplicationUser>()
                    .WithOne()
                    .HasForeignKey<DonorProfile>(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}