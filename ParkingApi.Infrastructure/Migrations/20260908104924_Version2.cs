using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParkingApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Version2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FullDayCoverageMinutes",
                table: "VehicleRates",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FullDayRulesJson",
                table: "Branches",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FullDayCoverageMinutes",
                table: "VehicleRates");

            migrationBuilder.DropColumn(
                name: "FullDayRulesJson",
                table: "Branches");
        }
    }
}
