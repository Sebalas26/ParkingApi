using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParkingApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Version3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FullDayRatesJson",
                table: "VehicleRates",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FullDayRatesJson",
                table: "VehicleRates");
        }
    }
}
