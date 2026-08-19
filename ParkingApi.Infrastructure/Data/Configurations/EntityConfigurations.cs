using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ParkingApi.Domain.Models;

namespace ParkingApi.Infrastructure.Data.Configurations;

public class EntityConfigurations :
    IEntityTypeConfiguration<User>,
    IEntityTypeConfiguration<Role>,
    IEntityTypeConfiguration<UserSession>,
    IEntityTypeConfiguration<VehicleRate>,
    IEntityTypeConfiguration<Store>,
    IEntityTypeConfiguration<CommercialAgreement>,
    IEntityTypeConfiguration<ParkingTicket>,
    IEntityTypeConfiguration<TicketDiscount>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(u => u.UserId);
        builder.Property(u => u.Username).IsRequired().HasMaxLength(50);
        builder.Property(u => u.FullName).IsRequired().HasMaxLength(150);
        builder.Property(u => u.PasswordHash).IsRequired().HasMaxLength(255);
        builder.HasIndex(u => u.Username).IsUnique();

        builder.HasOne(u => u.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");
        builder.HasKey(r => r.RoleId);
        builder.Property(r => r.Name).IsRequired().HasMaxLength(50);
    }

    public void Configure(EntityTypeBuilder<UserSession> builder)
    {
        builder.ToTable("UserSessions");
        builder.HasKey(s => s.SessionId);
        builder.Property(s => s.SessionToken).IsRequired().HasMaxLength(255);
    }

    public void Configure(EntityTypeBuilder<VehicleRate> builder)
    {
        builder.ToTable("VehicleRates");
        builder.HasKey(r => r.RateId);
        builder.Property(r => r.DisplayName).IsRequired().HasMaxLength(50);
        builder.Property(r => r.HourRate).HasPrecision(18, 2);
        builder.Property(r => r.MinuteRate).HasPrecision(18, 2);
        builder.Property(r => r.FullDayRate).HasPrecision(18, 2);
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
        builder.HasKey(a => a.AgreementId);
        builder.Property(a => a.Name).IsRequired().HasMaxLength(100);
        builder.Property(a => a.MinPurchaseAmount).HasPrecision(18, 2);
        builder.Property(a => a.DiscountPercentage).HasPrecision(5, 2);
        builder.Property(a => a.DiscountFixedAmount).HasPrecision(18, 2);

        builder.HasOne(a => a.Store)
            .WithMany(s => s.Agreements)
            .HasForeignKey(a => a.StoreId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    public void Configure(EntityTypeBuilder<ParkingTicket> builder)
    {
        builder.ToTable("ParkingTickets");
        builder.HasKey(t => t.TicketId);
        builder.Property(t => t.TicketNumber).IsRequired().HasMaxLength(50);
        builder.Property(t => t.PlateNumber).IsRequired().HasMaxLength(20);
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
        builder.HasKey(d => d.TicketDiscountId);
        builder.Property(d => d.InvoiceNumber).IsRequired().HasMaxLength(50);
        builder.Property(d => d.PurchaseAmount).HasPrecision(18, 2);
        builder.Property(d => d.AppliedDiscountAmount).HasPrecision(18, 2);

        builder.HasOne(d => d.Ticket)
            .WithMany(t => t.Discounts)
            .HasForeignKey(d => d.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(d => d.Store)
            .WithMany(s => s.TicketDiscounts)
            .HasForeignKey(d => d.StoreId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.Agreement)
            .WithMany(a => a.TicketDiscounts)
            .HasForeignKey(d => d.AgreementId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
