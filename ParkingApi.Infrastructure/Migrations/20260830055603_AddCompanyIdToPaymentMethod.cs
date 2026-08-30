using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParkingApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyIdToPaymentMethod : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "PaymentMethod",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMethod_CompanyId",
                table: "PaymentMethod",
                column: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentMethod_Companies_CompanyId",
                table: "PaymentMethod",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentMethod_Companies_CompanyId",
                table: "PaymentMethod");

            migrationBuilder.DropIndex(
                name: "IX_PaymentMethod_CompanyId",
                table: "PaymentMethod");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "PaymentMethod");
        }
    }
}
