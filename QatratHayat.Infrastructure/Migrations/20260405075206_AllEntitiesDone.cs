using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QatratHayat.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AllEntitiesDone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_donorProfiles_AspNetUsers_UserId",
                table: "donorProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_Hospitals_Branches_BranchId",
                table: "Hospitals");

            migrationBuilder.DropPrimaryKey(
                name: "PK_donorProfiles",
                table: "donorProfiles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BloodRequesters",
                table: "BloodRequesters");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Hospitals");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "ContactInfo",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "WorkingHours",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "BloodRequesters");

            migrationBuilder.RenameTable(
                name: "donorProfiles",
                newName: "DonorProfiles");

            migrationBuilder.RenameTable(
                name: "BloodRequesters",
                newName: "Beneficiaries");

            migrationBuilder.RenameColumn(
                name: "Address",
                table: "Hospitals",
                newName: "AddressEn");

            migrationBuilder.RenameIndex(
                name: "IX_donorProfiles_UserId",
                table: "DonorProfiles",
                newName: "IX_DonorProfiles_UserId");

            migrationBuilder.RenameColumn(
                name: "Address",
                table: "Branches",
                newName: "AddressEn");

            migrationBuilder.AddColumn<string>(
                name: "AddressAR",
                table: "Hospitals",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "HospitalNameAr",
                table: "Hospitals",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "HospitalNameEn",
                table: "Hospitals",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DescriptionAr",
                table: "Campaigns",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DescriptionEn",
                table: "Campaigns",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TitleAr",
                table: "Campaigns",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TitleEn",
                table: "Campaigns",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<int>(
                name: "ManagerUserId",
                table: "Branches",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "GPSLng",
                table: "Branches",
                type: "decimal(9,6)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(9,6)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "GPSLat",
                table: "Branches",
                type: "decimal(9,6)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(9,6)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressAr",
                table: "Branches",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BranchNameAR",
                table: "Branches",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BranchNameEn",
                table: "Branches",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Branches",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Branches",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NationalId",
                table: "Beneficiaries",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<string>(
                name: "FullNameAr",
                table: "Beneficiaries",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FullNameEn",
                table: "Beneficiaries",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DonorProfiles",
                table: "DonorProfiles",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Beneficiaries",
                table: "Beneficiaries",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OldValues = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    NewValues = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    IPAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLogs_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BloodRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RelationshipType = table.Column<int>(type: "int", nullable: false),
                    BloodType = table.Column<int>(type: "int", nullable: false),
                    UnitsNeeded = table.Column<int>(type: "int", nullable: false),
                    UrgencyLevel = table.Column<int>(type: "int", nullable: false),
                    RequestStatus = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClinicalNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DoctorApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancellationReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ShortagePhase1At = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ShortagePhase2At = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BeneficiaryId = table.Column<int>(type: "int", nullable: false),
                    HospitalId = table.Column<int>(type: "int", nullable: false),
                    RequesterUserId = table.Column<int>(type: "int", nullable: false),
                    DoctorId = table.Column<int>(type: "int", nullable: false),
                    CancelledByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BloodRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BloodRequests_AspNetUsers_CancelledByUserId",
                        column: x => x.CancelledByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BloodRequests_AspNetUsers_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BloodRequests_AspNetUsers_RequesterUserId",
                        column: x => x.RequesterUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BloodRequests_Beneficiaries_BeneficiaryId",
                        column: x => x.BeneficiaryId,
                        principalTable: "Beneficiaries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BloodRequests_Hospitals_HospitalId",
                        column: x => x.HospitalId,
                        principalTable: "Hospitals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BranchWorkingHours",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false),
                    OpenTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    CloseTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    IsClosed = table.Column<bool>(type: "bit", nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BranchWorkingHours", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BranchWorkingHours_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NotificationType = table.Column<int>(type: "int", nullable: false),
                    NotificationChannel = table.Column<int>(type: "int", nullable: false),
                    TitleAr = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    TitleEn = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    ContentAr = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ContentEn = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    NotificationStatus = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LinkedEntityType = table.Column<int>(type: "int", nullable: true),
                    LinkedEntityId = table.Column<int>(type: "int", nullable: true),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RecipientUserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_AspNetUsers_RecipientUserId",
                        column: x => x.RecipientUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ScreeningQuestions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TextAr = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    TextEn = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ScreeningLevel = table.Column<int>(type: "int", nullable: false),
                    DeferralType = table.Column<int>(type: "int", nullable: false),
                    IsConditional = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ConditionalDateLabel = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DeferralPeriodDays = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScreeningQuestions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DonationIntents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DonationType = table.Column<int>(type: "int", nullable: false),
                    DonationIntentStatus = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DonorProfileId = table.Column<int>(type: "int", nullable: false),
                    CampaignId = table.Column<int>(type: "int", nullable: true),
                    BloodRequestId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DonationIntents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DonationIntents_BloodRequests_BloodRequestId",
                        column: x => x.BloodRequestId,
                        principalTable: "BloodRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DonationIntents_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DonationIntents_DonorProfiles_DonorProfileId",
                        column: x => x.DonorProfileId,
                        principalTable: "DonorProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Donations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DonationType = table.Column<int>(type: "int", nullable: false),
                    InitialEligibilityStatus = table.Column<int>(type: "int", nullable: false),
                    FinalEligibilityStatus = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FinalDecisionReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DonorProfileId = table.Column<int>(type: "int", nullable: false),
                    EmployeeUserId = table.Column<int>(type: "int", nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    BloodRequestId = table.Column<int>(type: "int", nullable: true),
                    CampaignId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Donations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Donations_AspNetUsers_EmployeeUserId",
                        column: x => x.EmployeeUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Donations_BloodRequests_BloodRequestId",
                        column: x => x.BloodRequestId,
                        principalTable: "BloodRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Donations_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Donations_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Donations_DonorProfiles_DonorProfileId",
                        column: x => x.DonorProfileId,
                        principalTable: "DonorProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DeferralRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeferralType = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DecisionSource = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DonorProfileId = table.Column<int>(type: "int", nullable: false),
                    ScreeningQuestionId = table.Column<int>(type: "int", nullable: true),
                    DecidedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeferralRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeferralRecords_AspNetUsers_DecidedByUserId",
                        column: x => x.DecidedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DeferralRecords_DonorProfiles_DonorProfileId",
                        column: x => x.DonorProfileId,
                        principalTable: "DonorProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeferralRecords_ScreeningQuestions_ScreeningQuestionId",
                        column: x => x.ScreeningQuestionId,
                        principalTable: "ScreeningQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ScreeningAnswers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionType = table.Column<int>(type: "int", nullable: false),
                    Answer = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ConditionalDateValue = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    DonationIntentId = table.Column<int>(type: "int", nullable: true),
                    DonorProfileId = table.Column<int>(type: "int", nullable: true),
                    ScreeningQuestionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScreeningAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScreeningAnswers_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScreeningAnswers_DonationIntents_DonationIntentId",
                        column: x => x.DonationIntentId,
                        principalTable: "DonationIntents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScreeningAnswers_DonorProfiles_DonorProfileId",
                        column: x => x.DonorProfileId,
                        principalTable: "DonorProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScreeningAnswers_ScreeningQuestions_ScreeningQuestionId",
                        column: x => x.ScreeningQuestionId,
                        principalTable: "ScreeningQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BloodUnits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UnitCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    BloodType = table.Column<int>(type: "int", nullable: false),
                    CollectionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UnitStatus = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AllocatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DisposalDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DisposalReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DeallocationNote = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AllocatedToRequestId = table.Column<int>(type: "int", nullable: true),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    DonationId = table.Column<int>(type: "int", nullable: false),
                    DisposedByEmployeeId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BloodUnits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BloodUnits_AspNetUsers_DisposedByEmployeeId",
                        column: x => x.DisposedByEmployeeId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BloodUnits_BloodRequests_AllocatedToRequestId",
                        column: x => x.AllocatedToRequestId,
                        principalTable: "BloodRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BloodUnits_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BloodUnits_Donations_DonationId",
                        column: x => x.DonationId,
                        principalTable: "Donations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DonorProfiles_BloodTypeConfirmedByEmployeeId",
                table: "DonorProfiles",
                column: "BloodTypeConfirmedByEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_BranchId",
                table: "Campaigns",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_CreatedByUserId",
                table: "Campaigns",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Branches_ManagerUserId",
                table: "Branches",
                column: "ManagerUserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_BranchId",
                table: "AspNetUsers",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_HospitalId",
                table: "AspNetUsers",
                column: "HospitalId");

            migrationBuilder.CreateIndex(
                name: "IX_Beneficiaries_MergedIntoUserId",
                table: "Beneficiaries",
                column: "MergedIntoUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Beneficiaries_UserId",
                table: "Beneficiaries",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId",
                table: "AuditLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_BloodRequests_BeneficiaryId",
                table: "BloodRequests",
                column: "BeneficiaryId");

            migrationBuilder.CreateIndex(
                name: "IX_BloodRequests_CancelledByUserId",
                table: "BloodRequests",
                column: "CancelledByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BloodRequests_DoctorId",
                table: "BloodRequests",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_BloodRequests_HospitalId",
                table: "BloodRequests",
                column: "HospitalId");

            migrationBuilder.CreateIndex(
                name: "IX_BloodRequests_RequesterUserId",
                table: "BloodRequests",
                column: "RequesterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BloodUnits_AllocatedToRequestId",
                table: "BloodUnits",
                column: "AllocatedToRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_BloodUnits_BranchId",
                table: "BloodUnits",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_BloodUnits_DisposedByEmployeeId",
                table: "BloodUnits",
                column: "DisposedByEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_BloodUnits_DonationId",
                table: "BloodUnits",
                column: "DonationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BranchWorkingHours_BranchId",
                table: "BranchWorkingHours",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_DeferralRecords_DecidedByUserId",
                table: "DeferralRecords",
                column: "DecidedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DeferralRecords_DonorProfileId",
                table: "DeferralRecords",
                column: "DonorProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_DeferralRecords_ScreeningQuestionId",
                table: "DeferralRecords",
                column: "ScreeningQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_DonationIntents_BloodRequestId",
                table: "DonationIntents",
                column: "BloodRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_DonationIntents_CampaignId",
                table: "DonationIntents",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_DonationIntents_DonorProfileId",
                table: "DonationIntents",
                column: "DonorProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Donations_BloodRequestId",
                table: "Donations",
                column: "BloodRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_Donations_BranchId",
                table: "Donations",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Donations_CampaignId",
                table: "Donations",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_Donations_DonorProfileId",
                table: "Donations",
                column: "DonorProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Donations_EmployeeUserId",
                table: "Donations",
                column: "EmployeeUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_RecipientUserId",
                table: "Notifications",
                column: "RecipientUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ScreeningAnswers_DonationIntentId",
                table: "ScreeningAnswers",
                column: "DonationIntentId");

            migrationBuilder.CreateIndex(
                name: "IX_ScreeningAnswers_DonorProfileId",
                table: "ScreeningAnswers",
                column: "DonorProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ScreeningAnswers_ScreeningQuestionId",
                table: "ScreeningAnswers",
                column: "ScreeningQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_ScreeningAnswers_UserId",
                table: "ScreeningAnswers",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Branches_BranchId",
                table: "AspNetUsers",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Hospitals_HospitalId",
                table: "AspNetUsers",
                column: "HospitalId",
                principalTable: "Hospitals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Beneficiaries_AspNetUsers_MergedIntoUserId",
                table: "Beneficiaries",
                column: "MergedIntoUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Beneficiaries_AspNetUsers_UserId",
                table: "Beneficiaries",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Branches_AspNetUsers_ManagerUserId",
                table: "Branches",
                column: "ManagerUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Campaigns_AspNetUsers_CreatedByUserId",
                table: "Campaigns",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Campaigns_Branches_BranchId",
                table: "Campaigns",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DonorProfiles_AspNetUsers_BloodTypeConfirmedByEmployeeId",
                table: "DonorProfiles",
                column: "BloodTypeConfirmedByEmployeeId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DonorProfiles_AspNetUsers_UserId",
                table: "DonorProfiles",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Hospitals_Branches_BranchId",
                table: "Hospitals",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Branches_BranchId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Hospitals_HospitalId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Beneficiaries_AspNetUsers_MergedIntoUserId",
                table: "Beneficiaries");

            migrationBuilder.DropForeignKey(
                name: "FK_Beneficiaries_AspNetUsers_UserId",
                table: "Beneficiaries");

            migrationBuilder.DropForeignKey(
                name: "FK_Branches_AspNetUsers_ManagerUserId",
                table: "Branches");

            migrationBuilder.DropForeignKey(
                name: "FK_Campaigns_AspNetUsers_CreatedByUserId",
                table: "Campaigns");

            migrationBuilder.DropForeignKey(
                name: "FK_Campaigns_Branches_BranchId",
                table: "Campaigns");

            migrationBuilder.DropForeignKey(
                name: "FK_DonorProfiles_AspNetUsers_BloodTypeConfirmedByEmployeeId",
                table: "DonorProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_DonorProfiles_AspNetUsers_UserId",
                table: "DonorProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_Hospitals_Branches_BranchId",
                table: "Hospitals");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "BloodUnits");

            migrationBuilder.DropTable(
                name: "BranchWorkingHours");

            migrationBuilder.DropTable(
                name: "DeferralRecords");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "ScreeningAnswers");

            migrationBuilder.DropTable(
                name: "Donations");

            migrationBuilder.DropTable(
                name: "DonationIntents");

            migrationBuilder.DropTable(
                name: "ScreeningQuestions");

            migrationBuilder.DropTable(
                name: "BloodRequests");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DonorProfiles",
                table: "DonorProfiles");

            migrationBuilder.DropIndex(
                name: "IX_DonorProfiles_BloodTypeConfirmedByEmployeeId",
                table: "DonorProfiles");

            migrationBuilder.DropIndex(
                name: "IX_Campaigns_BranchId",
                table: "Campaigns");

            migrationBuilder.DropIndex(
                name: "IX_Campaigns_CreatedByUserId",
                table: "Campaigns");

            migrationBuilder.DropIndex(
                name: "IX_Branches_ManagerUserId",
                table: "Branches");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_BranchId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_HospitalId",
                table: "AspNetUsers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Beneficiaries",
                table: "Beneficiaries");

            migrationBuilder.DropIndex(
                name: "IX_Beneficiaries_MergedIntoUserId",
                table: "Beneficiaries");

            migrationBuilder.DropIndex(
                name: "IX_Beneficiaries_UserId",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "AddressAR",
                table: "Hospitals");

            migrationBuilder.DropColumn(
                name: "HospitalNameAr",
                table: "Hospitals");

            migrationBuilder.DropColumn(
                name: "HospitalNameEn",
                table: "Hospitals");

            migrationBuilder.DropColumn(
                name: "DescriptionAr",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "DescriptionEn",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "TitleAr",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "TitleEn",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "AddressAr",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "BranchNameAR",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "BranchNameEn",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "FullNameAr",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "FullNameEn",
                table: "Beneficiaries");

            migrationBuilder.RenameTable(
                name: "DonorProfiles",
                newName: "donorProfiles");

            migrationBuilder.RenameTable(
                name: "Beneficiaries",
                newName: "BloodRequesters");

            migrationBuilder.RenameColumn(
                name: "AddressEn",
                table: "Hospitals",
                newName: "Address");

            migrationBuilder.RenameIndex(
                name: "IX_DonorProfiles_UserId",
                table: "donorProfiles",
                newName: "IX_donorProfiles_UserId");

            migrationBuilder.RenameColumn(
                name: "AddressEn",
                table: "Branches",
                newName: "Address");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Hospitals",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Campaigns",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Campaigns",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<int>(
                name: "ManagerUserId",
                table: "Branches",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<decimal>(
                name: "GPSLng",
                table: "Branches",
                type: "decimal(9,6)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(9,6)");

            migrationBuilder.AlterColumn<decimal>(
                name: "GPSLat",
                table: "Branches",
                type: "decimal(9,6)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(9,6)");

            migrationBuilder.AddColumn<string>(
                name: "ContactInfo",
                table: "Branches",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Branches",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "WorkingHours",
                table: "Branches",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NationalId",
                table: "BloodRequesters",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "BloodRequesters",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_donorProfiles",
                table: "donorProfiles",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BloodRequesters",
                table: "BloodRequesters",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_donorProfiles_AspNetUsers_UserId",
                table: "donorProfiles",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Hospitals_Branches_BranchId",
                table: "Hospitals",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
