using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QatratHayat.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class fixBloodRequestV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_BloodRequests_RejectedByUserId",
                table: "BloodRequests",
                column: "RejectedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_BloodRequests_AspNetUsers_RejectedByUserId",
                table: "BloodRequests",
                column: "RejectedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BloodRequests_AspNetUsers_RejectedByUserId",
                table: "BloodRequests");

            migrationBuilder.DropIndex(
                name: "IX_BloodRequests_RejectedByUserId",
                table: "BloodRequests");
        }
    }
}
