using Microsoft.EntityFrameworkCore;
using ParkingApi.Domain.Models;
using Action = ParkingApi.Domain.Models.Action;
using Module = ParkingApi.Domain.Models.Module;

namespace ParkingApi.Infrastructure.Data;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
    }

    // Módulo Seguridad y Parametrización (Migración ApiTaller)
    public DbSet<User> User { get; set; }
    public DbSet<UserRole> UserRole { get; set; }
    public DbSet<Module> Module { get; set; }
    public DbSet<Operation> Operation { get; set; }
    public DbSet<Action> Action { get; set; }
    public DbSet<RoleAction> RoleAction { get; set; }
    public DbSet<UserRoleModule> UserRoleModule { get; set; }
    public DbSet<IdentificationType> IdentificationType { get; set; }
    public DbSet<PaymentMethod> PaymentMethod { get; set; }
    public DbSet<Login> Login { get; set; }
    public DbSet<PasswordResetToken> PasswordResetToken { get; set; }

    // Módulo Multi-Sede / Multi-Parqueadero
    public DbSet<Branch> Branches { get; set; }
    public DbSet<UserBranch> UserBranches { get; set; }
    public DbSet<BranchPaymentMethod> BranchPaymentMethods { get; set; }

    // Módulo Negocio Parqueadero (Preservación 100%)
    public DbSet<ParkingLot> ParkingLots { get; set; }
    public DbSet<UserParking> UserParkings { get; set; }
    public DbSet<VehicleRate> VehicleRates { get; set; }
    public DbSet<Store> Stores { get; set; }
    public DbSet<CommercialAgreement> CommercialAgreements { get; set; }
    public DbSet<ParkingTicket> ParkingTickets { get; set; }
    public DbSet<TicketDiscount> TicketDiscounts { get; set; }
    public DbSet<WorkShift> WorkShifts { get; set; }
    public DbSet<MonthlySubscription> MonthlySubscriptions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DataContext).Assembly);
    }
}
