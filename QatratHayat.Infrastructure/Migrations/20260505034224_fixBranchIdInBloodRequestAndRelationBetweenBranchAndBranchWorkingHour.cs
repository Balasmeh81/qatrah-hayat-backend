using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QatratHayat.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class fixBranchIdInBloodRequestAndRelationBetweenBranchAndBranchWorkingHour : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BranchWorkingHours_BranchId",
                table: "BranchWorkingHours");

            migrationBuilder.DropColumn(
                name: "ShortagePhase1At",
                table: "BloodRequests");

            migrationBuilder.RenameColumn(
                name: "ShortagePhase2At",
                table: "BloodRequests",
                newName: "PublishedAt");

            migrationBuilder.AddColumn<int>(
                name: "BranchId",
                table: "BloodRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PublishedByUserId",
                table: "BloodRequests",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BranchWorkingHours_BranchId_DayOfWeek",
                table: "BranchWorkingHours",
                columns: new[] { "BranchId", "DayOfWeek" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BloodUnits_UnitCode",
                table: "BloodUnits",
                column: "UnitCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BloodRequests_BranchId",
                table: "BloodRequests",
                column: "BranchId");

            migrationBuilder.AddForeignKey(
                name: "FK_BloodRequests_Branches_BranchId",
                table: "BloodRequests",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BloodRequests_Branches_BranchId",
                table: "BloodRequests");

            migrationBuilder.DropIndex(
                name: "IX_BranchWorkingHours_BranchId_DayOfWeek",
                table: "BranchWorkingHours");

            migrationBuilder.DropIndex(
                name: "IX_BloodUnits_UnitCode",
                table: "BloodUnits");

            migrationBuilder.DropIndex(
                name: "IX_BloodRequests_BranchId",
                table: "BloodRequests");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "BloodRequests");

            migrationBuilder.DropColumn(
                name: "PublishedByUserId",
                table: "BloodRequests");

            migrationBuilder.RenameColumn(
                name: "PublishedAt",
                table: "BloodRequests",
                newName: "ShortagePhase2At");

            migrationBuilder.AddColumn<DateTime>(
                name: "ShortagePhase1At",
                table: "BloodRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BranchWorkingHours_BranchId",
                table: "BranchWorkingHours",
                column: "BranchId");
        }
    }
}
