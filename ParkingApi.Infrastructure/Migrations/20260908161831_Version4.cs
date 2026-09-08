using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParkingApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Version4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EntryGracePeriodMinutes",
                table: "Branches",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ExitGracePeriodMinutes",
                table: "Branches",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EntryGracePeriodMinutes",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "ExitGracePeriodMinutes",
                table: "Branches");
        }
    }
}
