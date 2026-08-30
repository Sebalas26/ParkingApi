using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ParkingApi.Domain.Interfaces.Repositories.Companies;
using ParkingApi.Domain.Models;

namespace ParkingApi.Infrastructure.Data.Repositories.Companies;

public class CompanyRepository : ICompanyRepository
{
    private readonly DataContext _context;

    public CompanyRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Company>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Companies
            .AsNoTracking()
            .Include(c => c.Branches)
            .Include(c => c.Users)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Company>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Companies
            .AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Company?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Companies
            .Include(c => c.Branches)
            .Include(c => c.Users)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Company?> GetByNitAsync(string nit, CancellationToken cancellationToken = default)
    {
        return await _context.Companies
            .FirstOrDefaultAsync(c => c.Nit.ToLower() == nit.ToLower(), cancellationToken);
    }

    public async Task<Company> AddAsync(Company company, CancellationToken cancellationToken = default)
    {
        _context.Companies.Add(company);
        await _context.SaveChangesAsync(cancellationToken);
        return company;
    }

    public async Task<Company> UpdateAsync(Company company, CancellationToken cancellationToken = default)
    {
        _context.Companies.Update(company);
        await _context.SaveChangesAsync(cancellationToken);
        return company;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Companies.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (existing != null)
        {
            _context.Companies.Remove(existing);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
        return false;
    }
}
