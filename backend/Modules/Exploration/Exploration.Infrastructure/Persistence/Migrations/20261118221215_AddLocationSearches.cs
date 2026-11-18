using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Exploration.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLocationSearches : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LocationSearches",
                schema: "exploration",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Center_Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Center_Longitude = table.Column<double>(type: "double precision", nullable: false),
                    Radius_Kilometers = table.Column<double>(type: "double precision", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Difficulties = table.Column<string[]>(type: "text[]", nullable: true),
                    Tags = table.Column<List<string>>(type: "text[]", nullable: true),
                    Transport = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    MinPrice = table.Column<int>(type: "integer", nullable: true),
                    MaxPrice = table.Column<int>(type: "integer", nullable: true),
                    MinLength = table.Column<decimal>(type: "numeric", nullable: true),
                    MaxLength = table.Column<decimal>(type: "numeric", nullable: true),
                    ResultCount = table.Column<int>(type: "integer", nullable: false),
                    SearchedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationSearches", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LocationSearches",
                schema: "exploration");
        }
    }
}
