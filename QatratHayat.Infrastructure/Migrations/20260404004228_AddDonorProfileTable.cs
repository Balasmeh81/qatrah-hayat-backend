using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QatratHayat.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDonorProfileTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "donorProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    BloodType = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    BloodTypeStatus = table.Column<int>(type: "int", nullable: false),
                    BloodTypeConfirmedByEmployeeId = table.Column<int>(type: "int", nullable: true),
                    BloodTypeConfirmedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EligibilityStatus = table.Column<int>(type: "int", nullable: false),
                    PermanentDeferralReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LastDonationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NextEligibleDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DonationCount = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_donorProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_donorProfiles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_donorProfiles_UserId",
                table: "donorProfiles",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "donorProfiles");
        }
    }
}
