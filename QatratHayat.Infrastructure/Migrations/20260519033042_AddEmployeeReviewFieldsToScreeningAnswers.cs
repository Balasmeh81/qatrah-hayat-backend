using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QatratHayat.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeReviewFieldsToScreeningAnswers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmployeeReviewNotes",
                table: "ScreeningAnswers",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReviewedAdditionalText",
                table: "ScreeningAnswers",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ReviewedAnswer",
                table: "ScreeningAnswers",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedAt",
                table: "ScreeningAnswers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReviewedByEmployeeId",
                table: "ScreeningAnswers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedConditionalDateValue",
                table: "ScreeningAnswers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScreeningAnswers_ReviewedByEmployeeId",
                table: "ScreeningAnswers",
                column: "ReviewedByEmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_ScreeningAnswers_AspNetUsers_ReviewedByEmployeeId",
                table: "ScreeningAnswers",
                column: "ReviewedByEmployeeId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScreeningAnswers_AspNetUsers_ReviewedByEmployeeId",
                table: "ScreeningAnswers");

            migrationBuilder.DropIndex(
                name: "IX_ScreeningAnswers_ReviewedByEmployeeId",
                table: "ScreeningAnswers");

            migrationBuilder.DropColumn(
                name: "EmployeeReviewNotes",
                table: "ScreeningAnswers");

            migrationBuilder.DropColumn(
                name: "ReviewedAdditionalText",
                table: "ScreeningAnswers");

            migrationBuilder.DropColumn(
                name: "ReviewedAnswer",
                table: "ScreeningAnswers");

            migrationBuilder.DropColumn(
                name: "ReviewedAt",
                table: "ScreeningAnswers");

            migrationBuilder.DropColumn(
                name: "ReviewedByEmployeeId",
                table: "ScreeningAnswers");

            migrationBuilder.DropColumn(
                name: "ReviewedConditionalDateValue",
                table: "ScreeningAnswers");
        }
    }
}
