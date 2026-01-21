using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DateSpark.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAdventureCardsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdventureCards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdeaId = table.Column<int>(type: "integer", nullable: true),
                    CoupleId = table.Column<int>(type: "integer", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PlannedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: false),
                    PhotoUrl = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdventureCards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdventureCards_Couples_CoupleId",
                        column: x => x.CoupleId,
                        principalTable: "Couples",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AdventureCards_Ideas_IdeaId",
                        column: x => x.IdeaId,
                        principalTable: "Ideas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AdventureCards_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdventureCards_CoupleId",
                table: "AdventureCards",
                column: "CoupleId");

            migrationBuilder.CreateIndex(
                name: "IX_AdventureCards_CreatedByUserId",
                table: "AdventureCards",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AdventureCards_IdeaId",
                table: "AdventureCards",
                column: "IdeaId");

            migrationBuilder.CreateIndex(
                name: "IX_AdventureCards_PlannedDate",
                table: "AdventureCards",
                column: "PlannedDate");

            migrationBuilder.CreateIndex(
                name: "IX_AdventureCards_Status",
                table: "AdventureCards",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdventureCards");

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
    }
}
