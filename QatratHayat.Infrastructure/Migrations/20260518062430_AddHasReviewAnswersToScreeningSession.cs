using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QatratHayat.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHasReviewAnswersToScreeningSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasReviewAnswers",
                table: "ScreeningSessions",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasReviewAnswers",
                table: "ScreeningSessions");
        }
    }
}
