using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ParkingApi.Domain.Interfaces.Repositories.Billing;
using ParkingApi.Domain.Models;

namespace ParkingApi.Infrastructure.Data.Repositories.Billing;

public class DianDocumentTypeRepository : IDianDocumentTypeRepository
{
    private readonly DataContext _context;

    public DianDocumentTypeRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<DianDocumentType>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.DianDocumentTypes
            .AsNoTracking()
            .OrderBy(d => d.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DianDocumentType>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.DianDocumentTypes
            .AsNoTracking()
            .Where(d => d.IsActive)
            .OrderBy(d => d.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<DianDocumentType?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.DianDocumentTypes
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<DianDocumentType?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _context.DianDocumentTypes
            .FirstOrDefaultAsync(d => d.Code.ToLower() == code.ToLower(), cancellationToken);
    }

    public async Task<DianDocumentType> AddAsync(DianDocumentType entity, CancellationToken cancellationToken = default)
    {
        _context.DianDocumentTypes.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(DianDocumentType entity, CancellationToken cancellationToken = default)
    {
        _context.DianDocumentTypes.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(DianDocumentType entity, CancellationToken cancellationToken = default)
    {
        _context.DianDocumentTypes.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
