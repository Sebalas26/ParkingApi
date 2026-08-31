using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParkingApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBranchIdToUserRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BranchId",
                table: "UserRole",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserRole_BranchId",
                table: "UserRole",
                column: "BranchId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserRole_Branches_BranchId",
                table: "UserRole",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserRole_Branches_BranchId",
                table: "UserRole");

            migrationBuilder.DropIndex(
                name: "IX_UserRole_BranchId",
                table: "UserRole");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "UserRole");
        }
    }
}
