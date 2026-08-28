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
        builder.HasIndex(u => u.CompanyId);

        builder.HasOne(u => u.Company)
            .WithMany(c => c.Users)
            .HasForeignKey(u => u.CompanyId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(u => u.UserRoleIdNavigation)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.UserRoleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(u => u.IdentificationTypeIdNavigation)
            .WithMany(i => i.Users)
            .HasForeignKey(u => u.IdentificationTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("UserRole");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Role).IsRequired().HasMaxLength(50);

        builder.HasIndex(r => r.CompanyId);

        builder.HasOne(r => r.Company)
            .WithMany(c => c.UserRoles)
            .HasForeignKey(r => r.CompanyId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.ResponsibleUserIdNavigation)
            .WithMany()
            .HasForeignKey(r => r.ResponsibleUserId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
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

public class MultiBranchConfigurations :
    IEntityTypeConfiguration<Company>,
    IEntityTypeConfiguration<Branch>,
    IEntityTypeConfiguration<UserBranch>,
    IEntityTypeConfiguration<BranchPaymentMethod>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("Companies");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).IsRequired().HasMaxLength(150);
        builder.Property(c => c.LegalName).HasMaxLength(150);
        builder.Property(c => c.Nit).IsRequired().HasMaxLength(50);
        builder.Property(c => c.Email).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Phone).HasMaxLength(30);
        builder.Property(c => c.Address).HasMaxLength(200);
        builder.Property(c => c.City).HasMaxLength(50);
        builder.Property(c => c.PlanType).IsRequired().HasMaxLength(50).HasDefaultValue("Basic");
        builder.Property(c => c.MaxBranches).HasDefaultValue(1);
        builder.Ignore(c => c.LogoBase64);

        builder.HasIndex(c => c.Nit);

        builder.HasOne(c => c.ResponsibleUserIdNavigation)
            .WithMany()
            .HasForeignKey(c => c.ResponsibleUserId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.ToTable("Branches");
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Code).IsRequired().HasMaxLength(20);
        builder.Property(b => b.Name).IsRequired().HasMaxLength(100);
        builder.Property(b => b.Address).IsRequired().HasMaxLength(200);
        builder.Property(b => b.Phone).HasMaxLength(30);
        builder.Property(b => b.City).HasMaxLength(50);
        builder.Property(b => b.Notes).HasMaxLength(500);

        builder.HasIndex(b => b.Code).IsUnique();
        builder.HasIndex(b => b.CompanyId);

        builder.HasOne(b => b.Company)
            .WithMany(c => c.Branches)
            .HasForeignKey(b => b.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.ResponsibleUserIdNavigation)
            .WithMany()
            .HasForeignKey(b => b.ResponsibleUserId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<UserBranch> builder)
    {
        builder.ToTable("UserBranches");
        builder.HasKey(ub => ub.Id);

        builder.HasIndex(ub => new { ub.UserId, ub.BranchId }).IsUnique();

        builder.HasOne(ub => ub.User)
            .WithMany(u => u.UserBranches)
            .HasForeignKey(ub => ub.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ub => ub.Branch)
            .WithMany(b => b.UserBranches)
            .HasForeignKey(ub => ub.BranchId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    public void Configure(EntityTypeBuilder<BranchPaymentMethod> builder)
    {
        builder.ToTable("BranchPaymentMethods");
        builder.HasKey(bpm => bpm.Id);

        builder.HasIndex(bpm => new { bpm.BranchId, bpm.PaymentMethodId }).IsUnique();

        builder.HasOne(bpm => bpm.Branch)
            .WithMany(b => b.BranchPaymentMethods)
            .HasForeignKey(bpm => bpm.BranchId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(bpm => bpm.PaymentMethod)
            .WithMany()
            .HasForeignKey(bpm => bpm.PaymentMethodId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ParkingBusinessConfigurations :
    IEntityTypeConfiguration<VehicleRate>,
    IEntityTypeConfiguration<Store>,
    IEntityTypeConfiguration<CommercialAgreement>,
    IEntityTypeConfiguration<ParkingTicket>,
    IEntityTypeConfiguration<TicketDiscount>,
    IEntityTypeConfiguration<WorkShift>,
    IEntityTypeConfiguration<MonthlySubscription>,
    IEntityTypeConfiguration<BillingResolution>,
    IEntityTypeConfiguration<VehicleIncident>,
    IEntityTypeConfiguration<VehicleIncidentBranch>
{
    public void Configure(EntityTypeBuilder<VehicleRate> builder)
    {
        builder.ToTable("VehicleRates");
        builder.HasKey(r => r.RateId);
        builder.Property(r => r.DisplayName).IsRequired().HasMaxLength(50);
        builder.Property(r => r.IconKey).HasMaxLength(50);
        builder.Property(r => r.HourRate).HasPrecision(18, 2);
        builder.Property(r => r.MinuteRate).HasPrecision(18, 2);
        builder.Property(r => r.FullDayRate).HasPrecision(18, 2);

        builder.HasIndex(r => r.BranchId);
        builder.HasIndex(r => r.CompanyId);

        builder.HasOne(r => r.Company)
            .WithMany(c => c.VehicleRates)
            .HasForeignKey(r => r.CompanyId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Branch)
            .WithMany(b => b.VehicleRates)
            .HasForeignKey(r => r.BranchId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<Store> builder)
    {
        builder.ToTable("Stores");
        builder.HasKey(s => s.StoreId);
        builder.Property(s => s.Name).IsRequired().HasMaxLength(100);
        builder.Property(s => s.TaxId).IsRequired().HasMaxLength(50);

        builder.HasIndex(s => s.BranchId);
        builder.HasIndex(s => s.CompanyId);

        builder.HasOne(s => s.Company)
            .WithMany(c => c.Stores)
            .HasForeignKey(s => s.CompanyId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Branch)
            .WithMany(b => b.Stores)
            .HasForeignKey(s => s.BranchId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<CommercialAgreement> builder)
    {
        builder.ToTable("CommercialAgreements");
        builder.HasKey(ca => ca.AgreementId);
        builder.Property(ca => ca.Name).IsRequired().HasMaxLength(100);
        builder.Property(ca => ca.MinPurchaseAmount).HasPrecision(18, 2);
        builder.Property(ca => ca.DiscountPercentage).HasPrecision(5, 2);
        builder.Property(ca => ca.DiscountFixedAmount).HasPrecision(18, 2);
        builder.Property(ca => ca.ImageUrl).HasColumnType("longtext");

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

        builder.Property(t => t.ResolutionName).HasMaxLength(150);
        builder.Property(t => t.InvoiceNumber).HasMaxLength(50);

        builder.HasIndex(t => t.PlateNumber);
        builder.HasIndex(t => t.TicketNumber).IsUnique();
        builder.HasIndex(t => t.BranchId);
        builder.HasIndex(t => t.CompanyId);
        builder.HasIndex(t => t.ResolutionId);
        builder.HasIndex(t => t.IsElectronicInvoice);

        builder.HasOne(t => t.Company)
            .WithMany(c => c.ParkingTickets)
            .HasForeignKey(t => t.CompanyId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Branch)
            .WithMany(b => b.ParkingTickets)
            .HasForeignKey(t => t.BranchId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
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
        builder.HasIndex(ws => ws.BranchId);
        builder.HasIndex(ws => ws.CompanyId);

        builder.HasOne(ws => ws.Company)
            .WithMany(c => c.WorkShifts)
            .HasForeignKey(ws => ws.CompanyId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ws => ws.Branch)
            .WithMany(b => b.WorkShifts)
            .HasForeignKey(ws => ws.BranchId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ws => ws.User)
            .WithMany()
            .HasForeignKey(ws => ws.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<MonthlySubscription> builder)
    {
        builder.ToTable("MonthlySubscriptions");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.CustomerName).IsRequired().HasMaxLength(150);
        builder.Property(s => s.CustomerDocument).IsRequired().HasMaxLength(50);
        builder.Property(s => s.CustomerPhone).IsRequired().HasMaxLength(30);
        builder.Property(s => s.CustomerEmail).HasMaxLength(100);
        builder.Property(s => s.PlateNumber).IsRequired().HasMaxLength(20);
        builder.Property(s => s.Notes).HasMaxLength(500);

        builder.Property(s => s.MonthlyFee).HasPrecision(18, 2);
        builder.Property(s => s.AmountPaid).HasPrecision(18, 2);

        builder.HasIndex(s => s.PlateNumber);
        builder.HasIndex(s => s.SubscriptionId).IsUnique();
        builder.HasIndex(s => s.BranchId);
        builder.HasIndex(s => s.CompanyId);

        builder.HasOne(s => s.Company)
            .WithMany(c => c.MonthlySubscriptions)
            .HasForeignKey(s => s.CompanyId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Branch)
            .WithMany(b => b.MonthlySubscriptions)
            .HasForeignKey(s => s.BranchId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<BillingResolution> builder)
    {
        builder.ToTable("BillingResolutions");
        builder.HasKey(r => r.ResolutionId);
        builder.Property(r => r.Name).IsRequired().HasMaxLength(150);
        builder.Property(r => r.DocumentType).IsRequired().HasMaxLength(250);
        builder.Property(r => r.Prefix).IsRequired().HasMaxLength(20);
        builder.Property(r => r.ResolutionNumber).IsRequired().HasMaxLength(50);
        builder.Property(r => r.TechnicalKey).HasColumnType("longtext");

        builder.HasIndex(r => r.BranchId);
        builder.HasIndex(r => r.CompanyId);
        builder.HasIndex(r => r.ResolutionNumber);
        builder.HasIndex(r => r.Prefix);
        builder.HasIndex(r => r.IsActive);

        builder.HasOne(r => r.Company)
            .WithMany(c => c.BillingResolutions)
            .HasForeignKey(r => r.CompanyId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Branch)
            .WithMany()
            .HasForeignKey(r => r.BranchId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<VehicleIncident> builder)
    {
        builder.ToTable("VehicleIncidents");
        builder.HasKey(i => i.IncidentId);
        builder.Property(i => i.PlateNumber).IsRequired().HasMaxLength(20);
        builder.Property(i => i.IncidentType).IsRequired().HasMaxLength(100);
        builder.Property(i => i.Description).IsRequired().HasColumnType("longtext");
        builder.Property(i => i.ReportedBy).IsRequired().HasMaxLength(100);
        builder.Property(i => i.ContactPhone).HasMaxLength(30);
        builder.Property(i => i.Status).IsRequired().HasMaxLength(30);
        builder.Property(i => i.ResolvedNotes).HasColumnType("longtext");

        builder.HasIndex(i => i.PlateNumber);
        builder.HasIndex(i => i.BranchId);
        builder.HasIndex(i => i.CompanyId);
        builder.HasIndex(i => i.IsBlocked);
        builder.HasIndex(i => i.Status);

        builder.HasOne(i => i.Company)
            .WithMany(c => c.VehicleIncidents)
            .HasForeignKey(i => i.CompanyId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.Branch)
            .WithMany()
            .HasForeignKey(i => i.BranchId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<VehicleIncidentBranch> builder)
    {
        builder.ToTable("VehicleIncidentBranches");
        builder.HasKey(ib => new { ib.IncidentId, ib.BranchId });

        builder.HasOne(ib => ib.VehicleIncident)
            .WithMany(i => i.IncidentBranches)
            .HasForeignKey(ib => ib.IncidentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ib => ib.Branch)
            .WithMany()
            .HasForeignKey(ib => ib.BranchId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
