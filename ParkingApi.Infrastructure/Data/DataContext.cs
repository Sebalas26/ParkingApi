using Microsoft.EntityFrameworkCore;
using ParkingApi.Domain.Models;

namespace ParkingApi.Infrastructure.Data;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserSession> UserSessions => Set<UserSession>();
    public DbSet<VehicleRate> VehicleRates => Set<VehicleRate>();
    public DbSet<Store> Stores => Set<Store>();
    public DbSet<CommercialAgreement> CommercialAgreements => Set<CommercialAgreement>();
    public DbSet<ParkingTicket> ParkingTickets => Set<ParkingTicket>();
    public DbSet<TicketDiscount> TicketDiscounts => Set<TicketDiscount>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DataContext).Assembly);
    }
}
