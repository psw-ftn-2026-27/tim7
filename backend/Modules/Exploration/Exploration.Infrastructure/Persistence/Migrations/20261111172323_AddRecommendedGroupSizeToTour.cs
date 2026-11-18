using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Exploration.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRecommendedGroupSizeToTour : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RecommendedGroupSize_From",
                schema: "exploration",
                table: "Tours",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RecommendedGroupSize_To",
                schema: "exploration",
                table: "Tours",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RecommendedGroupSize_From",
                schema: "exploration",
                table: "Tours");

            migrationBuilder.DropColumn(
                name: "RecommendedGroupSize_To",
                schema: "exploration",
                table: "Tours");
        }
    }
}
