using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QatratHayat.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class fixSomeScreeningQuestions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ScreeningQuestions",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "AdditionalTextLabelAr", "AdditionalTextLabelEn", "RequiresAdditionalText" },
                values: new object[] { "", "", false });

            migrationBuilder.UpdateData(
                table: "ScreeningQuestions",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "AdditionalTextLabelAr", "AdditionalTextLabelEn", "DecisionMode", "DeferralType", "RequiresAdditionalText" },
                values: new object[] { "", "", 2, 2, false });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ScreeningQuestions",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "AdditionalTextLabelAr", "AdditionalTextLabelEn", "RequiresAdditionalText" },
                values: new object[] { "اذكر ما حدث", "Please specify what happened", true });

            migrationBuilder.UpdateData(
                table: "ScreeningQuestions",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "AdditionalTextLabelAr", "AdditionalTextLabelEn", "DecisionMode", "DeferralType", "RequiresAdditionalText" },
                values: new object[] { "اذكر اسم المرض", "Please specify the condition", 3, 3, true });
        }
    }
}
