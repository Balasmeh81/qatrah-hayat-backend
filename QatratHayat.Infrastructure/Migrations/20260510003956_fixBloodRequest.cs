using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QatratHayat.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class fixBloodRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "RejectedAt",
                table: "BloodRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RejectedByUserId",
                table: "BloodRequests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "BloodRequests",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RejectedAt",
                table: "BloodRequests");

            migrationBuilder.DropColumn(
                name: "RejectedByUserId",
                table: "BloodRequests");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "BloodRequests");
        }
    }
}
