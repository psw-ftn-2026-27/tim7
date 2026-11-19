using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Exploration.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTourProgress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "LastKnownLocation_Latitude",
                schema: "exploration",
                table: "TourExecutions",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "LastKnownLocation_Longitude",
                schema: "exploration",
                table: "TourExecutions",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.CreateTable(
                name: "KeyPointCompletions",
                schema: "exploration",
                columns: table => new
                {
                    TourExecutionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    KeyPointId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KeyPointCompletions", x => new { x.TourExecutionId, x.Id });
                    table.ForeignKey(
                        name: "FK_KeyPointCompletions_TourExecutions_TourExecutionId",
                        column: x => x.TourExecutionId,
                        principalSchema: "exploration",
                        principalTable: "TourExecutions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KeyPointCompletions",
                schema: "exploration");

            migrationBuilder.DropColumn(
                name: "LastKnownLocation_Latitude",
                schema: "exploration",
                table: "TourExecutions");

            migrationBuilder.DropColumn(
                name: "LastKnownLocation_Longitude",
                schema: "exploration",
                table: "TourExecutions");
        }
    }
}
