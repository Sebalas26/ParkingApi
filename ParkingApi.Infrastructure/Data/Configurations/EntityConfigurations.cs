using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ParkingApi.Domain.Models;

namespace ParkingApi.Infrastructure.Data.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");
        builder.HasKey(r => r.RoleId);
        builder.Property(r => r.Name).IsRequired().HasMaxLength(50);
        builder.Property(r => r.Description).HasMaxLength(250);
        builder.HasIndex(r => r.Name).IsUnique();
    }
}

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(u => u.UserId);
        builder.Property(u => u.Username).IsRequired().HasMaxLength(50);
        builder.Property(u => u.PasswordHash).IsRequired().HasMaxLength(255);
        builder.Property(u => u.FullName).IsRequired().HasMaxLength(150);
        builder.Property(u => u.Email).HasMaxLength(150);
        builder.HasIndex(u => u.Username).IsUnique();

        builder.HasOne(u => u.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
{
    public void Configure(EntityTypeBuilder<UserSession> builder)
    {
        builder.ToTable("UserSessions");
        builder.HasKey(s => s.SessionId);
        builder.Property(s => s.SessionToken).IsRequired().HasMaxLength(500);
        builder.Property(s => s.DeviceIdentifier).HasMaxLength(150).IsRequired(false);
        builder.Property(s => s.IpAddress).HasMaxLength(50).IsRequired(false);

        builder.HasOne(s => s.User)
            .WithMany(u => u.Sessions)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class PasswordResetTokenConfiguration : IEntityTypeConfiguration<PasswordResetToken>
{
    public void Configure(EntityTypeBuilder<PasswordResetToken> builder)
    {
        builder.ToTable("PasswordResetTokens");
        builder.HasKey(t => t.ResetTokenId);
        builder.Property(t => t.Token).IsRequired().HasMaxLength(255);
        builder.HasIndex(t => t.Token).IsUnique();

        builder.HasOne(t => t.User)
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class AppModuleConfiguration : IEntityTypeConfiguration<AppModule>
{
    public void Configure(EntityTypeBuilder<AppModule> builder)
    {
        builder.ToTable("AppModules");
        builder.HasKey(m => m.ModuleId);
        builder.Property(m => m.Code).IsRequired().HasMaxLength(50);
        builder.Property(m => m.Name).IsRequired().HasMaxLength(100);
        builder.Property(m => m.Icon).HasMaxLength(50);
        builder.HasIndex(m => m.Code).IsUnique();
    }
}

public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("RolePermissions");
        builder.HasKey(rp => rp.RolePermissionId);
        builder.Property(rp => rp.PermissionSlug).IsRequired().HasMaxLength(100);

        builder.HasOne(rp => rp.Role)
            .WithMany(r => r.RolePermissions)
            .HasForeignKey(rp => rp.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rp => rp.Module)
            .WithMany(m => m.RolePermissions)
            .HasForeignKey(rp => rp.ModuleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class VehicleRateConfiguration : IEntityTypeConfiguration<VehicleRate>
{
    public void Configure(EntityTypeBuilder<VehicleRate> builder)
    {
        builder.ToTable("VehicleRates");
        builder.HasKey(vr => vr.RateId);
        builder.Property(vr => vr.DisplayName).IsRequired().HasMaxLength(50);
        builder.Property(vr => vr.IconKey).HasMaxLength(50);
        builder.Property(vr => vr.HourRate).HasPrecision(18, 2);
        builder.Property(vr => vr.MinuteRate).HasPrecision(18, 2);
        builder.Property(vr => vr.FullDayRate).HasPrecision(18, 2);
    }
}

public class StoreConfiguration : IEntityTypeConfiguration<Store>
{
    public void Configure(EntityTypeBuilder<Store> builder)
    {
        builder.ToTable("Stores");
        builder.HasKey(s => s.StoreId);
        builder.Property(s => s.Name).IsRequired().HasMaxLength(100);
        builder.Property(s => s.TaxId).IsRequired().HasMaxLength(50);
        builder.Property(s => s.PhoneNumber).HasMaxLength(30);
        builder.Property(s => s.ContactName).HasMaxLength(100);
    }
}

public class CommercialAgreementConfiguration : IEntityTypeConfiguration<CommercialAgreement>
{
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
}

public class ParkingTicketConfiguration : IEntityTypeConfiguration<ParkingTicket>
{
    public void Configure(EntityTypeBuilder<ParkingTicket> builder)
    {
        builder.ToTable("ParkingTickets");
        builder.HasKey(pt => pt.TicketId);
        builder.Property(pt => pt.TicketNumber).IsRequired().HasMaxLength(50);
        builder.Property(pt => pt.PlateNumber).IsRequired().HasMaxLength(20);
        builder.Property(pt => pt.CustomerPhone).HasMaxLength(30);
        builder.Property(pt => pt.Notes).HasMaxLength(500);
        builder.Property(pt => pt.OperatorName).IsRequired().HasMaxLength(100);
        builder.Property(pt => pt.HourlyRate).HasPrecision(18, 2);
        builder.Property(pt => pt.GrossAmount).HasPrecision(18, 2);
        builder.Property(pt => pt.DiscountAmount).HasPrecision(18, 2);
        builder.Property(pt => pt.NetAmount).HasPrecision(18, 2);
        builder.Property(pt => pt.AmountPaid).HasPrecision(18, 2);
        builder.Property(pt => pt.ChangeGiven).HasPrecision(18, 2);

        builder.HasIndex(pt => pt.TicketNumber).IsUnique();
        builder.HasIndex(pt => pt.PlateNumber);
    }
}

public class TicketDiscountConfiguration : IEntityTypeConfiguration<TicketDiscount>
{
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
            .WithMany()
            .HasForeignKey(td => td.StoreId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(td => td.Agreement)
            .WithMany()
            .HasForeignKey(td => td.AgreementId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
