using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ParkingApi.Domain.Common.Enums;
using ParkingApi.Domain.Models;
using Action = ParkingApi.Domain.Models.Action;
using PaymentMethodModel = ParkingApi.Domain.Models.PaymentMethod;

namespace ParkingApi.Infrastructure.Data.Configurations;

public class SecurityConfigurations :
    IEntityTypeConfiguration<User>,
    IEntityTypeConfiguration<UserRole>,
    IEntityTypeConfiguration<Module>,
    IEntityTypeConfiguration<Operation>,
    IEntityTypeConfiguration<Action>,
    IEntityTypeConfiguration<RoleAction>,
    IEntityTypeConfiguration<UserRoleModule>,
    IEntityTypeConfiguration<IdentificationType>,
    IEntityTypeConfiguration<PaymentMethodModel>,
    IEntityTypeConfiguration<Login>,
    IEntityTypeConfiguration<PasswordResetToken>
{
    private static readonly DateTime BaseSeedDate = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly string AdminPasswordHash = ParkingApi.Infrastructure.Security.PasswordHasher.HashPassword("admin123");
    private static readonly string OperadorPasswordHash = ParkingApi.Infrastructure.Security.PasswordHasher.HashPassword("operador123");

    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("User");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Username).IsRequired().HasMaxLength(50);
        builder.Property(u => u.Password).IsRequired().HasMaxLength(255);
        builder.Property(u => u.FullName).IsRequired().HasMaxLength(150);
        builder.Property(u => u.Email).IsRequired().HasMaxLength(100);
        builder.Property(u => u.IdentificationNumber).IsRequired().HasMaxLength(50);

        builder.HasIndex(u => u.Username).IsUnique();

        builder.HasOne(u => u.UserRoleIdNavigation)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.UserRoleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(u => u.IdentificationTypeIdNavigation)
            .WithMany(i => i.Users)
            .HasForeignKey(u => u.IdentificationTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(
            new User
            {
                Id = 1,
                UserRoleId = 1,
                IdentificationTypeId = 1,
                IdentificationNumber = "1000000001",
                FirstName = "Administrador",
                MiddleName = string.Empty,
                FirstSurname = "Principal",
                SecondLastName = string.Empty,
                FullName = "Administrador del Sistema",
                Username = "admin",
                Password = AdminPasswordHash,
                Email = "admin@parkflow.com",
                IsActive = true,
                MustChangePassword = false,
                CreatedAt = BaseSeedDate
            },
            new User
            {
                Id = 2,
                UserRoleId = 2,
                IdentificationTypeId = 1,
                IdentificationNumber = "1000000002",
                FirstName = "Operador",
                MiddleName = string.Empty,
                FirstSurname = "Turno",
                SecondLastName = string.Empty,
                FullName = "Operador de Turno",
                Username = "operador",
                Password = OperadorPasswordHash,
                Email = "operador@parkflow.com",
                IsActive = true,
                MustChangePassword = false,
                CreatedAt = BaseSeedDate
            }
        );
    }

    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("UserRole");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Role).IsRequired().HasMaxLength(50);

        builder.HasOne(r => r.ResponsibleUserIdNavigation)
            .WithMany()
            .HasForeignKey(r => r.ResponsibleUserId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(
            new UserRole { Id = 1, Role = "Administrador", IsActive = true, CreatedAt = BaseSeedDate },
            new UserRole { Id = 2, Role = "Operador", IsActive = true, CreatedAt = BaseSeedDate }
        );
    }

    public void Configure(EntityTypeBuilder<Module> builder)
    {
        builder.ToTable("Module");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Name).IsRequired().HasMaxLength(100);

        builder.HasOne(m => m.ResponsibleUserIdNavigation)
            .WithMany()
            .HasForeignKey(m => m.ResponsibleUserId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(
            new Module { Id = 1, Name = "Seguridad", IsActive = true, CreatedAt = BaseSeedDate },
            new Module { Id = 2, Name = "Tiquetes", IsActive = true, CreatedAt = BaseSeedDate },
            new Module { Id = 3, Name = "Tarifas", IsActive = true, CreatedAt = BaseSeedDate },
            new Module { Id = 4, Name = "Comercios y Convenios", IsActive = true, CreatedAt = BaseSeedDate },
            new Module { Id = 5, Name = "Reportes y Métricas", IsActive = true, CreatedAt = BaseSeedDate },
            new Module { Id = 6, Name = "Sincronización", IsActive = true, CreatedAt = BaseSeedDate }
        );
    }

    public void Configure(EntityTypeBuilder<Operation> builder)
    {
        builder.ToTable("Operation");
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Name).IsRequired().HasMaxLength(100);

        builder.HasOne(o => o.ResponsibleUserIdNavigation)
            .WithMany()
            .HasForeignKey(o => o.ResponsibleUserId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(
            new Operation { Id = 1, Name = "Lectura", IsActive = true, CreatedAt = BaseSeedDate },
            new Operation { Id = 2, Name = "Creación", IsActive = true, CreatedAt = BaseSeedDate },
            new Operation { Id = 3, Name = "Edición", IsActive = true, CreatedAt = BaseSeedDate },
            new Operation { Id = 4, Name = "Eliminación", IsActive = true, CreatedAt = BaseSeedDate }
        );
    }

    public void Configure(EntityTypeBuilder<Action> builder)
    {
        builder.ToTable("Action");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Name).IsRequired().HasMaxLength(100);
        builder.Property(a => a.Slug).IsRequired().HasMaxLength(100);

        builder.HasOne(a => a.ModuleIdNavigation)
            .WithMany(m => m.Actions)
            .HasForeignKey(a => a.ModuleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.OperationIdNavigation)
            .WithMany(o => o.Actions)
            .HasForeignKey(a => a.OperationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.ResponsibleUserIdNavigation)
            .WithMany()
            .HasForeignKey(a => a.ResponsibleUserId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<RoleAction> builder)
    {
        builder.ToTable("RoleAction");
        builder.HasKey(ra => ra.Id);

        builder.HasOne(ra => ra.RoleIdNavigation)
            .WithMany(r => r.RoleActions)
            .HasForeignKey(ra => ra.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ra => ra.ActionIdNavigation)
            .WithMany(a => a.RoleActions)
            .HasForeignKey(ra => ra.ActionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ra => ra.ResponsibleUserIdNavigation)
            .WithMany()
            .HasForeignKey(ra => ra.ResponsibleUserId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<UserRoleModule> builder)
    {
        builder.ToTable("UserRoleModule");
        builder.HasKey(urm => urm.Id);

        builder.HasOne(urm => urm.UserRoleIdNavigation)
            .WithMany(r => r.UserRoleModules)
            .HasForeignKey(urm => urm.UserRoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(urm => urm.ModuleIdNavigation)
            .WithMany(m => m.UserRoleModules)
            .HasForeignKey(urm => urm.ModulesRoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(urm => urm.ResponsibleUserIdNavigation)
            .WithMany()
            .HasForeignKey(urm => urm.ResponsibleUserId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<IdentificationType> builder)
    {
        builder.ToTable("IdentificationType");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Identification).IsRequired().HasMaxLength(50);

        builder.HasOne(i => i.ResponsibleUserIdNavigation)
            .WithMany()
            .HasForeignKey(i => i.ResponsibleUserId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(
            new IdentificationType { Id = 1, Identification = "Cédula de Ciudadanía", IsActive = true, CreatedAt = BaseSeedDate },
            new IdentificationType { Id = 2, Identification = "Cédula de Extranjería", IsActive = true, CreatedAt = BaseSeedDate },
            new IdentificationType { Id = 3, Identification = "Tarjeta de Identidad", IsActive = true, CreatedAt = BaseSeedDate },
            new IdentificationType { Id = 4, Identification = "NIT", IsActive = true, CreatedAt = BaseSeedDate },
            new IdentificationType { Id = 5, Identification = "Pasaporte", IsActive = true, CreatedAt = BaseSeedDate }
        );
    }

    public void Configure(EntityTypeBuilder<PaymentMethodModel> builder)
    {
        builder.ToTable("PaymentMethod");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(50);
        builder.Property(p => p.Icon).HasMaxLength(50);

        builder.HasOne(p => p.ResponsibleUserIdNavigation)
            .WithMany()
            .HasForeignKey(p => p.ResponsibleUserId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(
            new PaymentMethodModel { Id = 1, Name = "Efectivo", Icon = "cash", IsActive = true, CreatedAt = BaseSeedDate },
            new PaymentMethodModel { Id = 2, Name = "Tarjeta Débito", Icon = "credit-card", IsActive = true, CreatedAt = BaseSeedDate },
            new PaymentMethodModel { Id = 3, Name = "Tarjeta Crédito", Icon = "credit-card", IsActive = true, CreatedAt = BaseSeedDate },
            new PaymentMethodModel { Id = 4, Name = "Transferencia / QR", Icon = "qrcode", IsActive = true, CreatedAt = BaseSeedDate }
        );
    }

    public void Configure(EntityTypeBuilder<Login> builder)
    {
        builder.ToTable("Login");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Message).HasMaxLength(255);

        builder.HasOne(l => l.User)
            .WithMany(u => u.Logins)
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    public void Configure(EntityTypeBuilder<PasswordResetToken> builder)
    {
        builder.ToTable("PasswordResetToken");
        builder.HasKey(pr => pr.Id);
        builder.Property(pr => pr.Token).IsRequired().HasMaxLength(255);

        builder.HasOne(pr => pr.User)
            .WithMany(u => u.PasswordResetTokens)
            .HasForeignKey(pr => pr.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ParkingBusinessConfigurations :
    IEntityTypeConfiguration<VehicleRate>,
    IEntityTypeConfiguration<Store>,
    IEntityTypeConfiguration<CommercialAgreement>,
    IEntityTypeConfiguration<ParkingTicket>,
    IEntityTypeConfiguration<TicketDiscount>,
    IEntityTypeConfiguration<WorkShift>
{
    private static readonly DateTime BaseSeedDate = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public void Configure(EntityTypeBuilder<VehicleRate> builder)
    {
        builder.ToTable("VehicleRates");
        builder.HasKey(r => r.RateId);
        builder.Property(r => r.DisplayName).IsRequired().HasMaxLength(50);
        builder.Property(r => r.IconKey).HasMaxLength(50);
        builder.Property(r => r.HourRate).HasPrecision(18, 2);
        builder.Property(r => r.MinuteRate).HasPrecision(18, 2);
        builder.Property(r => r.FullDayRate).HasPrecision(18, 2);

        builder.HasData(
            new VehicleRate
            {
                RateId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                VehicleType = VehicleType.Car,
                DisplayName = "Automóvil / Sedán",
                HourRate = 4000m,
                MinuteRate = 70m,
                FullDayRate = 28000m,
                GracePeriodMinutes = 15,
                IconKey = "IconCar",
                IsActive = true,
                CreatedAtUtc = BaseSeedDate
            },
            new VehicleRate
            {
                RateId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                VehicleType = VehicleType.Motorcycle,
                DisplayName = "Motocicleta",
                HourRate = 2000m,
                MinuteRate = 35m,
                FullDayRate = 14000m,
                GracePeriodMinutes = 15,
                IconKey = "IconMotorcycle",
                IsActive = true,
                CreatedAtUtc = BaseSeedDate
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
                CreatedAtUtc = BaseSeedDate
            },
            new VehicleRate
            {
                RateId = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                VehicleType = VehicleType.Van,
                DisplayName = "Furgón / Minibús",
                HourRate = 6000m,
                MinuteRate = 100m,
                FullDayRate = 42000m,
                GracePeriodMinutes = 15,
                IconKey = "IconVan",
                IsActive = true,
                CreatedAtUtc = BaseSeedDate
            },
            new VehicleRate
            {
                RateId = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                VehicleType = VehicleType.Truck,
                DisplayName = "Vehículo Pesado / Camión",
                HourRate = 10000m,
                MinuteRate = 170m,
                FullDayRate = 70000m,
                GracePeriodMinutes = 15,
                IconKey = "IconTruck",
                IsActive = true,
                CreatedAtUtc = BaseSeedDate
            },
            new VehicleRate
            {
                RateId = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                VehicleType = VehicleType.Bicycle,
                DisplayName = "Bicicleta",
                HourRate = 800m,
                MinuteRate = 15m,
                FullDayRate = 5000m,
                GracePeriodMinutes = 15,
                IconKey = "IconBicycle",
                IsActive = true,
                CreatedAtUtc = BaseSeedDate
            }
        );
    }

    public void Configure(EntityTypeBuilder<Store> builder)
    {
        builder.ToTable("Stores");
        builder.HasKey(s => s.StoreId);
        builder.Property(s => s.Name).IsRequired().HasMaxLength(100);
        builder.Property(s => s.TaxId).IsRequired().HasMaxLength(50);
    }

    public void Configure(EntityTypeBuilder<CommercialAgreement> builder)
    {
        builder.ToTable("CommercialAgreements");
        builder.HasKey(ca => ca.AgreementId);
        builder.Property(ca => ca.Name).IsRequired().HasMaxLength(100);
        builder.Property(ca => ca.MinPurchaseAmount).HasPrecision(18, 2);
        builder.Property(ca => ca.DiscountPercentage).HasPrecision(5, 2);
        builder.Property(ca => ca.DiscountFixedAmount).HasPrecision(18, 2);

        builder.HasOne(ca => ca.Store)
            .WithMany(s => s.Agreements)
            .HasForeignKey(ca => ca.StoreId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    public void Configure(EntityTypeBuilder<ParkingTicket> builder)
    {
        builder.ToTable("ParkingTickets");
        builder.HasKey(t => t.TicketId);
        builder.Property(t => t.TicketNumber).IsRequired().HasMaxLength(50);
        builder.Property(t => t.PlateNumber).IsRequired().HasMaxLength(20);
        builder.Property(t => t.CustomerPhone).HasMaxLength(30);
        builder.Property(t => t.Notes).HasMaxLength(500);
        builder.Property(t => t.OperatorName).IsRequired().HasMaxLength(100);
        builder.Property(t => t.HourlyRate).HasPrecision(18, 2);
        builder.Property(t => t.GrossAmount).HasPrecision(18, 2);
        builder.Property(t => t.DiscountAmount).HasPrecision(18, 2);
        builder.Property(t => t.NetAmount).HasPrecision(18, 2);
        builder.Property(t => t.AmountPaid).HasPrecision(18, 2);
        builder.Property(t => t.ChangeGiven).HasPrecision(18, 2);
        builder.HasIndex(t => t.PlateNumber);
        builder.HasIndex(t => t.TicketNumber).IsUnique();
    }

    public void Configure(EntityTypeBuilder<TicketDiscount> builder)
    {
        builder.ToTable("TicketDiscounts");
        builder.HasKey(td => td.TicketDiscountId);
        builder.Property(td => td.InvoiceNumber).IsRequired().HasMaxLength(50);
        builder.Property(td => td.PurchaseAmount).HasPrecision(18, 2);
        builder.Property(td => td.AppliedDiscountAmount).HasPrecision(18, 2);

        builder.HasOne(td => td.Ticket)
            .WithMany(t => t.Discounts)
            .HasForeignKey(td => td.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(td => td.Store)
            .WithMany(s => s.TicketDiscounts)
            .HasForeignKey(td => td.StoreId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(td => td.Agreement)
            .WithMany(a => a.TicketDiscounts)
            .HasForeignKey(td => td.AgreementId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<WorkShift> builder)
    {
        builder.ToTable("WorkShifts");
        builder.HasKey(ws => ws.ShiftId);
        builder.Property(ws => ws.OperatorName).IsRequired().HasMaxLength(100);
        builder.Property(ws => ws.BaseAmount).HasPrecision(18, 2);
        builder.Property(ws => ws.TotalCashCollected).HasPrecision(18, 2);
        builder.Property(ws => ws.TotalCardCollected).HasPrecision(18, 2);
        builder.Property(ws => ws.TotalTransferCollected).HasPrecision(18, 2);
        builder.Property(ws => ws.TotalDiscounts).HasPrecision(18, 2);
        builder.Property(ws => ws.ExpectedCash).HasPrecision(18, 2);
        builder.Property(ws => ws.ActualCashCounted).HasPrecision(18, 2);
        builder.Property(ws => ws.CashDifference).HasPrecision(18, 2);
        builder.Property(ws => ws.Notes).HasMaxLength(500);

        builder.HasIndex(ws => ws.UserId);
        builder.HasIndex(ws => ws.Status);

        builder.HasOne(ws => ws.User)
            .WithMany()
            .HasForeignKey(ws => ws.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
