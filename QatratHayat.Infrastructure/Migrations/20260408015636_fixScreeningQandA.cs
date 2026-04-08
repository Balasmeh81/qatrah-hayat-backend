using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QatratHayat.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class fixScreeningQandA : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ScreeningLevel",
                table: "ScreeningQuestions",
                newName: "SessionType");

            migrationBuilder.RenameColumn(
                name: "IsConditional",
                table: "ScreeningQuestions",
                newName: "IsForFemaleOnly");

            migrationBuilder.RenameColumn(
                name: "SessionType",
                table: "ScreeningAnswers",
                newName: "ScreeningSessionId");

            migrationBuilder.AddColumn<bool>(
                name: "AutoFailWhenYes",
                table: "ScreeningQuestions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "AdditionalText",
                table: "ScreeningAnswers",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ScreeningSessionId",
                table: "DeferralRecords",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ScreeningSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionType = table.Column<int>(type: "int", nullable: false),
                    ResultEligibilityStatus = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    DonorProfileId = table.Column<int>(type: "int", nullable: true),
                    DonationIntentId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScreeningSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScreeningSessions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScreeningSessions_DonationIntents_DonationIntentId",
                        column: x => x.DonationIntentId,
                        principalTable: "DonationIntents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScreeningSessions_DonorProfiles_DonorProfileId",
                        column: x => x.DonorProfileId,
                        principalTable: "DonorProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScreeningAnswers_ScreeningSessionId",
                table: "ScreeningAnswers",
                column: "ScreeningSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_DeferralRecords_ScreeningSessionId",
                table: "DeferralRecords",
                column: "ScreeningSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_ScreeningSessions_DonationIntentId",
                table: "ScreeningSessions",
                column: "DonationIntentId");

            migrationBuilder.CreateIndex(
                name: "IX_ScreeningSessions_DonorProfileId",
                table: "ScreeningSessions",
                column: "DonorProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ScreeningSessions_UserId",
                table: "ScreeningSessions",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_DeferralRecords_ScreeningSessions_ScreeningSessionId",
                table: "DeferralRecords",
                column: "ScreeningSessionId",
                principalTable: "ScreeningSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ScreeningAnswers_ScreeningSessions_ScreeningSessionId",
                table: "ScreeningAnswers",
                column: "ScreeningSessionId",
                principalTable: "ScreeningSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeferralRecords_ScreeningSessions_ScreeningSessionId",
                table: "DeferralRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_ScreeningAnswers_ScreeningSessions_ScreeningSessionId",
                table: "ScreeningAnswers");

            migrationBuilder.DropTable(
                name: "ScreeningSessions");

            migrationBuilder.DropIndex(
                name: "IX_ScreeningAnswers_ScreeningSessionId",
                table: "ScreeningAnswers");

            migrationBuilder.DropIndex(
                name: "IX_DeferralRecords_ScreeningSessionId",
                table: "DeferralRecords");

            migrationBuilder.DropColumn(
                name: "AutoFailWhenYes",
                table: "ScreeningQuestions");

            migrationBuilder.DropColumn(
                name: "AdditionalText",
                table: "ScreeningAnswers");

            migrationBuilder.DropColumn(
                name: "ScreeningSessionId",
                table: "DeferralRecords");

            migrationBuilder.RenameColumn(
                name: "SessionType",
                table: "ScreeningQuestions",
                newName: "ScreeningLevel");

            migrationBuilder.RenameColumn(
                name: "IsForFemaleOnly",
                table: "ScreeningQuestions",
                newName: "IsConditional");

            migrationBuilder.RenameColumn(
                name: "ScreeningSessionId",
                table: "ScreeningAnswers",
                newName: "SessionType");
        }
    }
}
