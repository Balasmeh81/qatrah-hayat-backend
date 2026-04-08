using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QatratHayat.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class fixScreeningQuestion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ConditionalDateLabel",
                table: "ScreeningQuestions",
                newName: "ConditionalDateLabelEn");

            migrationBuilder.RenameColumn(
                name: "AutoFailWhenYes",
                table: "ScreeningQuestions",
                newName: "RequiresDateValue");

            migrationBuilder.AddColumn<string>(
                name: "AdditionalTextLabelAr",
                table: "ScreeningQuestions",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AdditionalTextLabelEn",
                table: "ScreeningQuestions",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConditionalDateLabelAr",
                table: "ScreeningQuestions",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DecisionMode",
                table: "ScreeningQuestions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresAdditionalText",
                table: "ScreeningQuestions",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdditionalTextLabelAr",
                table: "ScreeningQuestions");

            migrationBuilder.DropColumn(
                name: "AdditionalTextLabelEn",
                table: "ScreeningQuestions");

            migrationBuilder.DropColumn(
                name: "ConditionalDateLabelAr",
                table: "ScreeningQuestions");

            migrationBuilder.DropColumn(
                name: "DecisionMode",
                table: "ScreeningQuestions");

            migrationBuilder.DropColumn(
                name: "RequiresAdditionalText",
                table: "ScreeningQuestions");

            migrationBuilder.RenameColumn(
                name: "RequiresDateValue",
                table: "ScreeningQuestions",
                newName: "AutoFailWhenYes");

            migrationBuilder.RenameColumn(
                name: "ConditionalDateLabelEn",
                table: "ScreeningQuestions",
                newName: "ConditionalDateLabel");
        }
    }
}
