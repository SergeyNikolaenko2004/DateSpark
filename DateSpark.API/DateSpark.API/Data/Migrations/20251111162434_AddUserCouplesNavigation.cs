using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DateSpark.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserCouplesNavigation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId1",
                table: "UserCouples",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserCouples_UserId1",
                table: "UserCouples",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_UserCouples_Users_UserId1",
                table: "UserCouples",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserCouples_Users_UserId1",
                table: "UserCouples");

            migrationBuilder.DropIndex(
                name: "IX_UserCouples_UserId1",
                table: "UserCouples");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "UserCouples");
        }
    }
}
