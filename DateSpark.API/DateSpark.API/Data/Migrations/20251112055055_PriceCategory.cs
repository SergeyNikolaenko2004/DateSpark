using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DateSpark.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class PriceCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Price",
                table: "Ideas");

            migrationBuilder.AddColumn<int>(
                name: "PriceCategory",
                table: "Ideas",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PriceCategory",
                table: "Ideas");

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Ideas",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);
        }
    }
}
