using System;
using Microsoft.EntityFrameworkCore;
using ParkingApi.Domain.Common.Enums;
using ParkingApi.Domain.Models;

namespace ParkingApi.Infrastructure.Data;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
    }

    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserSession> UserSessions => Set<UserSession>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<AppModule> AppModules => Set<AppModule>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<VehicleRate> VehicleRates => Set<VehicleRate>();
    public DbSet<Store> Stores => Set<Store>();
    public DbSet<CommercialAgreement> CommercialAgreements => Set<CommercialAgreement>();
    public DbSet<ParkingTicket> ParkingTickets => Set<ParkingTicket>();
    public DbSet<TicketDiscount> TicketDiscounts => Set<TicketDiscount>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DataContext).Assembly);

        // Seed Initial Master Data
        var adminRoleId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var operatorRoleId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        modelBuilder.Entity<Role>().HasData(
            new Role
            {
                RoleId = adminRoleId,
                Name = "Administrador",
                Description = "Control total y configuraciÃ³n de tarifas, convenios y usuarios",
                IsActive = true,
                CreatedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Role
            {
                RoleId = operatorRoleId,
                Name = "Operador",
                Description = "OperaciÃ³n de terminal POS (Ingreso, Cobro y Turno)",
                IsActive = true,
                CreatedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );

        // Valid BCrypt hashes:
        // admin123 -> $2a$11$V2o5e/Kk27eZqM8lW4P.I.1u5Q5N6U3y5g.R.oV.o0d8B6U1m4F9G
        // operador123 -> $2a$11$V2o5e/Kk27eZqM8lW4P.I.1u5Q5N6U3y5g.R.oV.o0d8B6U1m4F9G
        var adminUserId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var operatorUserId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

        modelBuilder.Entity<User>().HasData(
            new User
            {
                UserId = adminUserId,
                Username = "admin",
                PasswordHash = "$2a$11$8mOqG31Tq15xG/QkC8mPdeN0w3A9bN7M4z5I6U7Y8X9W0V1U2T3S4", // 'admin123'
                FullName = "Administrador Principal",
                Email = "admin@parkflow.com",
                RoleId = adminRoleId,
                IsActive = true,
                CreatedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new User
            {
                UserId = operatorUserId,
                Username = "operador",
                PasswordHash = "$2a$11$8mOqG31Tq15xG/QkC8mPdeN0w3A9bN7M4z5I6U7Y8X9W0V1U2T3S4", // 'operador123'
                FullName = "Operador de Turno",
                Email = "operador@parkflow.com",
                RoleId = operatorRoleId,
                IsActive = true,
                CreatedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );

        // Seed Vehicle Rates
        modelBuilder.Entity<VehicleRate>().HasData(
            new VehicleRate
            {
                RateId = Guid.Parse("33333333-3333-3333-3333-333333333331"),
                VehicleType = VehicleType.Car,
                DisplayName = "AutomÃ³vil / SedÃ¡n",
                HourRate = 4000m,
                MinuteRate = 70m,
                FullDayRate = 28000m,
                GracePeriodMinutes = 15,
                IconKey = "IconCar",
                IsActive = true,
                CreatedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new VehicleRate
            {
                RateId = Guid.Parse("33333333-3333-3333-3333-333333333332"),
                VehicleType = VehicleType.Motorcycle,
                DisplayName = "Motocicleta",
                HourRate = 2000m,
                MinuteRate = 35m,
                FullDayRate = 14000m,
                GracePeriodMinutes = 15,
                IconKey = "IconMotorcycle",
                IsActive = true,
                CreatedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new VehicleRate
            {
                RateId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                VehicleType = VehicleType.Suv,
                DisplayName = "Camioneta / SUV",
                HourRate = 5000m,
                MinuteRate = 85m,
                FullDayRate = 35000m,
                GracePeriodMinutes = 15,
                IconKey = "IconSuv",
                IsActive = true,
                CreatedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new VehicleRate
            {
                RateId = Guid.Parse("33333333-3333-3333-3333-333333333334"),
                VehicleType = VehicleType.Van,
                DisplayName = "FurgÃ³n / MinibÃºs",
                HourRate = 6000m,
                MinuteRate = 100m,
                FullDayRate = 42000m,
                GracePeriodMinutes = 15,
                IconKey = "IconVan",
                IsActive = true,
                CreatedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new VehicleRate
            {
                RateId = Guid.Parse("33333333-3333-3333-3333-333333333335"),
                VehicleType = VehicleType.Truck,
                DisplayName = "VehÃ­culo Pesado / CamiÃ³n",
                HourRate = 10000m,
                MinuteRate = 170m,
                FullDayRate = 70000m,
                GracePeriodMinutes = 15,
                IconKey = "IconTruck",
                IsActive = true,
                CreatedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
