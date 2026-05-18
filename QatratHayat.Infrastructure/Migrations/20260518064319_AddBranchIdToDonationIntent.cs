using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QatratHayat.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBranchIdToDonationIntent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DonationIntents_DonorProfileId",
                table: "DonationIntents");

            migrationBuilder.AddColumn<int>(
                name: "BranchId",
                table: "DonationIntents",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_DonationIntents_BranchId",
                table: "DonationIntents",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_DonationIntents_DonorProfileId_DonationIntentStatus",
                table: "DonationIntents",
                columns: new[] { "DonorProfileId", "DonationIntentStatus" });

            migrationBuilder.AddForeignKey(
                name: "FK_DonationIntents_Branches_BranchId",
                table: "DonationIntents",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DonationIntents_Branches_BranchId",
                table: "DonationIntents");

            migrationBuilder.DropIndex(
                name: "IX_DonationIntents_BranchId",
                table: "DonationIntents");

            migrationBuilder.DropIndex(
                name: "IX_DonationIntents_DonorProfileId_DonationIntentStatus",
                table: "DonationIntents");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "DonationIntents");

            migrationBuilder.CreateIndex(
                name: "IX_DonationIntents_DonorProfileId",
                table: "DonationIntents",
                column: "DonorProfileId");
        }
    }
}
