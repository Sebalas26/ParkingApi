using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ParkingApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class VersionBase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ParkingTickets",
                columns: table => new
                {
                    TicketId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TicketNumber = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PlateNumber = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    VehicleType = table.Column<int>(type: "int", nullable: false),
                    CustomerPhone = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Notes = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EntryTimeUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ExitTimeUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    TotalDurationMinutes = table.Column<int>(type: "int", nullable: false),
                    HourlyRate = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    GrossAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    NetAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AmountPaid = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ChangeGiven = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PaymentMethod = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    OperatorName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsSynchronized = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParkingTickets", x => x.TicketId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Stores",
                columns: table => new
                {
                    StoreId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TaxId = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PhoneNumber = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stores", x => x.StoreId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "VehicleRates",
                columns: table => new
                {
                    RateId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    VehicleType = table.Column<int>(type: "int", nullable: false),
                    DisplayName = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HourRate = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MinuteRate = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    FullDayRate = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    GracePeriodMinutes = table.Column<int>(type: "int", nullable: false),
                    IconKey = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleRates", x => x.RateId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CommercialAgreements",
                columns: table => new
                {
                    AgreementId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    StoreId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MinPurchaseAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DiscountPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    DiscountFixedAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    MaxHoursApplicable = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommercialAgreements", x => x.AgreementId);
                    table.ForeignKey(
                        name: "FK_CommercialAgreements_Stores_StoreId",
                        column: x => x.StoreId,
                        principalTable: "Stores",
                        principalColumn: "StoreId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TicketDiscounts",
                columns: table => new
                {
                    TicketDiscountId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TicketId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    StoreId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    AgreementId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    InvoiceNumber = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PurchaseAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AppliedDiscountAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ValidatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    IsSynchronized = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketDiscounts", x => x.TicketDiscountId);
                    table.ForeignKey(
                        name: "FK_TicketDiscounts_CommercialAgreements_AgreementId",
                        column: x => x.AgreementId,
                        principalTable: "CommercialAgreements",
                        principalColumn: "AgreementId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TicketDiscounts_ParkingTickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "ParkingTickets",
                        principalColumn: "TicketId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TicketDiscounts_Stores_StoreId",
                        column: x => x.StoreId,
                        principalTable: "Stores",
                        principalColumn: "StoreId",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Action",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ModuleId = table.Column<int>(type: "int", nullable: false),
                    OperationId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Slug = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ResponsibleUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Action", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "IdentificationType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Identification = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ResponsibleUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdentificationType", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Login",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Message = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ResponsibleUserId = table.Column<int>(type: "int", nullable: true),
                    ResponsibleUserIdNavigationId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Login", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Module",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ResponsibleUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Module", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Operation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ResponsibleUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Operation", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PasswordResetToken",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Token = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ExpirationDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    IsUsed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ResponsibleUserId = table.Column<int>(type: "int", nullable: true),
                    ResponsibleUserIdNavigationId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordResetToken", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PaymentMethod",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Icon = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ResponsibleUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentMethod", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RoleAction",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    ActionId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ResponsibleUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleAction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleAction_Action_ActionId",
                        column: x => x.ActionId,
                        principalTable: "Action",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserRoleId = table.Column<int>(type: "int", nullable: false),
                    IdentificationTypeId = table.Column<int>(type: "int", nullable: false),
                    IdentificationNumber = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FirstName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MiddleName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FirstSurname = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SecondLastName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FullName = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Username = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Password = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Token = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AssignmentDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ExpirationDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    MustChangePassword = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                    table.ForeignKey(
                        name: "FK_User_IdentificationType_IdentificationTypeId",
                        column: x => x.IdentificationTypeId,
                        principalTable: "IdentificationType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UserRole",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Role = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ResponsibleUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRole", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRole_User_ResponsibleUserId",
                        column: x => x.ResponsibleUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "WorkShifts",
                columns: table => new
                {
                    ShiftId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    OperatorName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StartTimeUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    EndTimeUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    BaseAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalCashCollected = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalCardCollected = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalTransferCollected = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalDiscounts = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ExpectedCash = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ActualCashCounted = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CashDifference = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalTicketsProcessed = table.Column<int>(type: "int", nullable: false),
                    TotalVehiclesEntered = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ClosedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkShifts", x => x.ShiftId);
                    table.ForeignKey(
                        name: "FK_WorkShifts_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UserRoleModule",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserRoleId = table.Column<int>(type: "int", nullable: false),
                    ModulesRoleId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ResponsibleUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoleModule", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRoleModule_Module_ModulesRoleId",
                        column: x => x.ModulesRoleId,
                        principalTable: "Module",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRoleModule_UserRole_UserRoleId",
                        column: x => x.UserRoleId,
                        principalTable: "UserRole",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRoleModule_User_ResponsibleUserId",
                        column: x => x.ResponsibleUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "IdentificationType",
                columns: new[] { "Id", "CreatedAt", "Identification", "IsActive", "ResponsibleUserId", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Cédula de Ciudadanía", true, null, null },
                    { 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Cédula de Extranjería", true, null, null },
                    { 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Tarjeta de Identidad", true, null, null },
                    { 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "NIT", true, null, null },
                    { 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Pasaporte", true, null, null }
                });

            migrationBuilder.InsertData(
                table: "Module",
                columns: new[] { "Id", "CreatedAt", "IsActive", "Name", "ResponsibleUserId", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "Seguridad", null, null },
                    { 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "Tiquetes", null, null },
                    { 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "Tarifas", null, null },
                    { 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "Comercios y Convenios", null, null },
                    { 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "Reportes y Métricas", null, null },
                    { 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "Sincronización", null, null }
                });

            migrationBuilder.InsertData(
                table: "Operation",
                columns: new[] { "Id", "CreatedAt", "IsActive", "Name", "ResponsibleUserId", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "Lectura", null, null },
                    { 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "Creación", null, null },
                    { 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "Edición", null, null },
                    { 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "Eliminación", null, null }
                });

            migrationBuilder.InsertData(
                table: "PaymentMethod",
                columns: new[] { "Id", "CreatedAt", "Icon", "IsActive", "Name", "ResponsibleUserId", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "cash", true, "Efectivo", null, null },
                    { 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "credit-card", true, "Tarjeta Débito", null, null },
                    { 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "credit-card", true, "Tarjeta Crédito", null, null },
                    { 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "qrcode", true, "Transferencia / QR", null, null }
                });

            migrationBuilder.InsertData(
                table: "UserRole",
                columns: new[] { "Id", "CreatedAt", "IsActive", "ResponsibleUserId", "Role", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, null, "Administrador", null },
                    { 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, null, "Operador", null }
                });

            migrationBuilder.InsertData(
                table: "VehicleRates",
                columns: new[] { "RateId", "CreatedAtUtc", "DisplayName", "FullDayRate", "GracePeriodMinutes", "HourRate", "IconKey", "IsActive", "MinuteRate", "UpdatedAtUtc", "VehicleType" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Automóvil / Sedán", 28000m, 15, 4000m, "IconCar", true, 70m, null, 0 },
                    { new Guid("22222222-2222-2222-2222-222222222222"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Motocicleta", 14000m, 15, 2000m, "IconMotorcycle", true, 35m, null, 1 },
                    { new Guid("33333333-3333-3333-3333-333333333333"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Camioneta / SUV", 35000m, 15, 5000m, "IconSuv", true, 85m, null, 5 },
                    { new Guid("44444444-4444-4444-4444-444444444444"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Furgón / Minibús", 42000m, 15, 6000m, "IconVan", true, 100m, null, 3 },
                    { new Guid("55555555-5555-5555-5555-555555555555"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Vehículo Pesado / Camión", 70000m, 15, 10000m, "IconTruck", true, 170m, null, 2 },
                    { new Guid("66666666-6666-6666-6666-666666666666"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Bicicleta", 5000m, 15, 800m, "IconBicycle", true, 15m, null, 4 }
                });

            migrationBuilder.InsertData(
                table: "User",
                columns: new[] { "Id", "AssignmentDate", "CreatedAt", "Email", "ExpirationDate", "FirstName", "FirstSurname", "FullName", "IdentificationNumber", "IdentificationTypeId", "IsActive", "MiddleName", "MustChangePassword", "Password", "SecondLastName", "Token", "UpdatedAt", "UserRoleId", "Username" },
                values: new object[,]
                {
                    { 1, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "admin@parkflow.com", null, "Administrador", "Principal", "Administrador del Sistema", "1000000001", 1, true, "", false, "$2a$11$AFLUWi4qDaKPVko1OopOEu5FIh8Fu9f94pby4uqKPzwGJfZvqO93y", "", null, null, 1, "admin" },
                    { 2, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "operador@parkflow.com", null, "Operador", "Turno", "Operador de Turno", "1000000002", 1, true, "", false, "$2a$11$e2qqUTVWokInjCyxD5/.VOKMPlxVoPkXPcUaaktRVUYmZCM3hPys.", "", null, null, 2, "operador" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Action_ModuleId",
                table: "Action",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_Action_OperationId",
                table: "Action",
                column: "OperationId");

            migrationBuilder.CreateIndex(
                name: "IX_Action_ResponsibleUserId",
                table: "Action",
                column: "ResponsibleUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CommercialAgreements_StoreId",
                table: "CommercialAgreements",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_IdentificationType_ResponsibleUserId",
                table: "IdentificationType",
                column: "ResponsibleUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Login_ResponsibleUserIdNavigationId",
                table: "Login",
                column: "ResponsibleUserIdNavigationId");

            migrationBuilder.CreateIndex(
                name: "IX_Login_UserId",
                table: "Login",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Module_ResponsibleUserId",
                table: "Module",
                column: "ResponsibleUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Operation_ResponsibleUserId",
                table: "Operation",
                column: "ResponsibleUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ParkingTickets_PlateNumber",
                table: "ParkingTickets",
                column: "PlateNumber");

            migrationBuilder.CreateIndex(
                name: "IX_ParkingTickets_TicketNumber",
                table: "ParkingTickets",
                column: "TicketNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetToken_ResponsibleUserIdNavigationId",
                table: "PasswordResetToken",
                column: "ResponsibleUserIdNavigationId");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetToken_UserId",
                table: "PasswordResetToken",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMethod_ResponsibleUserId",
                table: "PaymentMethod",
                column: "ResponsibleUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleAction_ActionId",
                table: "RoleAction",
                column: "ActionId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleAction_ResponsibleUserId",
                table: "RoleAction",
                column: "ResponsibleUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleAction_RoleId",
                table: "RoleAction",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketDiscounts_AgreementId",
                table: "TicketDiscounts",
                column: "AgreementId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketDiscounts_StoreId",
                table: "TicketDiscounts",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketDiscounts_TicketId",
                table: "TicketDiscounts",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_User_IdentificationTypeId",
                table: "User",
                column: "IdentificationTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_User_Username",
                table: "User",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_UserRoleId",
                table: "User",
                column: "UserRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRole_ResponsibleUserId",
                table: "UserRole",
                column: "ResponsibleUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoleModule_ModulesRoleId",
                table: "UserRoleModule",
                column: "ModulesRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoleModule_ResponsibleUserId",
                table: "UserRoleModule",
                column: "ResponsibleUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoleModule_UserRoleId",
                table: "UserRoleModule",
                column: "UserRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkShifts_Status",
                table: "WorkShifts",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_WorkShifts_UserId",
                table: "WorkShifts",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Action_Module_ModuleId",
                table: "Action",
                column: "ModuleId",
                principalTable: "Module",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Action_Operation_OperationId",
                table: "Action",
                column: "OperationId",
                principalTable: "Operation",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Action_User_ResponsibleUserId",
                table: "Action",
                column: "ResponsibleUserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_IdentificationType_User_ResponsibleUserId",
                table: "IdentificationType",
                column: "ResponsibleUserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Login_User_ResponsibleUserIdNavigationId",
                table: "Login",
                column: "ResponsibleUserIdNavigationId",
                principalTable: "User",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Login_User_UserId",
                table: "Login",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Module_User_ResponsibleUserId",
                table: "Module",
                column: "ResponsibleUserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Operation_User_ResponsibleUserId",
                table: "Operation",
                column: "ResponsibleUserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PasswordResetToken_User_ResponsibleUserIdNavigationId",
                table: "PasswordResetToken",
                column: "ResponsibleUserIdNavigationId",
                principalTable: "User",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PasswordResetToken_User_UserId",
                table: "PasswordResetToken",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentMethod_User_ResponsibleUserId",
                table: "PaymentMethod",
                column: "ResponsibleUserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RoleAction_UserRole_RoleId",
                table: "RoleAction",
                column: "RoleId",
                principalTable: "UserRole",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RoleAction_User_ResponsibleUserId",
                table: "RoleAction",
                column: "ResponsibleUserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_User_UserRole_UserRoleId",
                table: "User",
                column: "UserRoleId",
                principalTable: "UserRole",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IdentificationType_User_ResponsibleUserId",
                table: "IdentificationType");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRole_User_ResponsibleUserId",
                table: "UserRole");

            migrationBuilder.DropTable(
                name: "Login");

            migrationBuilder.DropTable(
                name: "PasswordResetToken");

            migrationBuilder.DropTable(
                name: "PaymentMethod");

            migrationBuilder.DropTable(
                name: "RoleAction");

            migrationBuilder.DropTable(
                name: "TicketDiscounts");

            migrationBuilder.DropTable(
                name: "UserRoleModule");

            migrationBuilder.DropTable(
                name: "VehicleRates");

            migrationBuilder.DropTable(
                name: "WorkShifts");

            migrationBuilder.DropTable(
                name: "Action");

            migrationBuilder.DropTable(
                name: "CommercialAgreements");

            migrationBuilder.DropTable(
                name: "ParkingTickets");

            migrationBuilder.DropTable(
                name: "Module");

            migrationBuilder.DropTable(
                name: "Operation");

            migrationBuilder.DropTable(
                name: "Stores");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "IdentificationType");

            migrationBuilder.DropTable(
                name: "UserRole");
        }
    }
}
