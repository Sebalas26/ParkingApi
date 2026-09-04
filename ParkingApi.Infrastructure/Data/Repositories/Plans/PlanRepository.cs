using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ParkingApi.Domain.Interfaces.Repositories.Plans;
using ParkingApi.Domain.Models;

namespace ParkingApi.Infrastructure.Data.Repositories.Plans;

public class PlanRepository : IPlanRepository
{
    private readonly DataContext _context;

    public PlanRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<SaaSPlan>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Plans
            .AsNoTracking()
            .Include(p => p.Companies)
            .OrderBy(p => p.PriceCop)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SaaSPlan>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Plans
            .AsNoTracking()
            .Include(p => p.Companies)
            .Where(p => p.IsActive)
            .OrderBy(p => p.PriceCop)
            .ToListAsync(cancellationToken);
    }

    public async Task<SaaSPlan?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Plans
            .Include(p => p.Companies)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<SaaSPlan> AddAsync(SaaSPlan plan, CancellationToken cancellationToken = default)
    {
        _context.Plans.Add(plan);
        await _context.SaveChangesAsync(cancellationToken);
        return plan;
    }

    public async Task<SaaSPlan> UpdateAsync(SaaSPlan plan, CancellationToken cancellationToken = default)
    {
        _context.Plans.Update(plan);
        await _context.SaveChangesAsync(cancellationToken);
        return plan;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Plans.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (existing == null) return false;

        _context.Plans.Remove(existing);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
