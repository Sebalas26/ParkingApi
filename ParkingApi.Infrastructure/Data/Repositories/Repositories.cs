using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ParkingApi.Domain.Interfaces.Repositories;
using ParkingApi.Domain.Models;
using ParkingApi.Infrastructure.Data;

namespace ParkingApi.Infrastructure.Data.Repositories;

public class BaseRepository<T> : IBaseRepository<T> where T : class
{
    protected readonly DataContext _context;
    protected readonly DbSet<T> _dbSet;

    public BaseRepository(DataContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(predicate).ToListAsync(cancellationToken);
    }

    public async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}

public class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(DataContext context) : base(context) { }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower() && u.IsActive, cancellationToken);
    }
}

public class ParkingTicketRepository : BaseRepository<ParkingTicket>, IParkingTicketRepository
{
    public ParkingTicketRepository(DataContext context) : base(context) { }

    public async Task<ParkingTicket?> GetActiveByPlateAsync(string plateNumber, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(t =>
            t.Status == Domain.Common.Enums.TicketStatus.Active &&
            t.PlateNumber.ToLower() == plateNumber.ToLower(), cancellationToken);
    }

    public async Task<IReadOnlyList<ParkingTicket>> GetActiveTicketsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t => t.Status == Domain.Common.Enums.TicketStatus.Active)
            .OrderByDescending(t => t.EntryTimeUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ParkingTicket>> GetTodayCompletedTicketsAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        return await _dbSet
            .Where(t => t.Status == Domain.Common.Enums.TicketStatus.Completed && t.ExitTimeUtc.HasValue && t.ExitTimeUtc.Value.Date == today)
            .OrderByDescending(t => t.ExitTimeUtc)
            .ToListAsync(cancellationToken);
    }
}

public class VehicleRateRepository : BaseRepository<VehicleRate>, IVehicleRateRepository
{
    public VehicleRateRepository(DataContext context) : base(context) { }

    public async Task<VehicleRate?> GetByTypeAsync(Domain.Common.Enums.VehicleType type, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(r => r.VehicleType == type && r.IsActive, cancellationToken);
    }
}

public class StoreRepository : BaseRepository<Store>, IStoreRepository
{
    public StoreRepository(DataContext context) : base(context) { }
}

public class AgreementRepository : BaseRepository<CommercialAgreement>, IAgreementRepository
{
    public AgreementRepository(DataContext context) : base(context) { }
}

public class UserSessionRepository : BaseRepository<UserSession>, IUserSessionRepository
{
    public UserSessionRepository(DataContext context) : base(context) { }
}
