using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Exploration.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPriceToTour : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Price_Amount",
                schema: "exploration",
                table: "Tours",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Price_IsFree",
                schema: "exploration",
                table: "Tours",
                type: "boolean",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Price_Amount",
                schema: "exploration",
                table: "Tours");

            migrationBuilder.DropColumn(
                name: "Price_IsFree",
                schema: "exploration",
                table: "Tours");
        }
    }
}
