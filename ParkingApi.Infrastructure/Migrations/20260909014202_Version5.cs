using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParkingApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Version5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UsersPerBranch",
                table: "Plans",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UsersPerBranch",
                table: "Plans");
        }
    }
}
