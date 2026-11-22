using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Games.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLocationChallengeProgress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LocationChallengeProgresses",
                schema: "games",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    LocationChallengeId = table.Column<Guid>(type: "uuid", nullable: false),
                    DiscoveredPointIds = table.Column<Guid[]>(type: "uuid[]", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EarnedXp = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationChallengeProgresses", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LocationChallengeProgresses_UserId_LocationChallengeId",
                schema: "games",
                table: "LocationChallengeProgresses",
                columns: new[] { "UserId", "LocationChallengeId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LocationChallengeProgresses",
                schema: "games");
        }
    }
}
