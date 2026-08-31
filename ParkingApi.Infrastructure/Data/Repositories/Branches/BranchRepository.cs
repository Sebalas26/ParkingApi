using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ParkingApi.Domain.Interfaces.Repositories.Branches;
using ParkingApi.Domain.Models;

namespace ParkingApi.Infrastructure.Data.Repositories.Branches;

public class BranchRepository : IBranchRepository
{
    private readonly DataContext _context;

    public BranchRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Branch>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Branches
            .AsNoTracking()
            .OrderBy(b => b.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Branch>> GetActiveAsync(int? companyId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Branches.AsNoTracking().Where(b => b.IsActive);
        if (companyId.HasValue && companyId.Value > 0)
        {
            query = query.Where(b => b.CompanyId == companyId.Value);
        }
        return await query.OrderBy(b => b.Name).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Branch>> GetBranchesByCompanyIdAsync(int companyId, CancellationToken cancellationToken = default)
    {
        return await _context.Branches
            .AsNoTracking()
            .Where(b => b.CompanyId == companyId && b.IsActive)
            .OrderBy(b => b.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Branch?> GetByIdAsync(int branchId, CancellationToken cancellationToken = default)
    {
        return await _context.Branches
            .Include(b => b.BranchPaymentMethods)
                .ThenInclude(bpm => bpm.PaymentMethod)
            .FirstOrDefaultAsync(b => b.Id == branchId, cancellationToken);
    }

    public async Task<Branch?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _context.Branches
            .FirstOrDefaultAsync(b => b.Code.ToLower() == code.ToLower(), cancellationToken);
    }

    public async Task<IReadOnlyList<Branch>> GetBranchesByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _context.UserBranches
            .AsNoTracking()
            .Where(ub => ub.UserId == userId && ub.IsActive && ub.Branch.IsActive)
            .Select(ub => ub.Branch)
            .OrderBy(b => b.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Branch> AddAsync(Branch branch, CancellationToken cancellationToken = default)
    {
        _context.Branches.Add(branch);
        await _context.SaveChangesAsync(cancellationToken);
        return branch;
    }

    public async Task<Branch> UpdateAsync(Branch branch, CancellationToken cancellationToken = default)
    {
        _context.Branches.Update(branch);
        await _context.SaveChangesAsync(cancellationToken);
        return branch;
    }

    public async Task<bool> AssignUserAsync(int userId, int branchId, bool isDefault, CancellationToken cancellationToken = default)
    {
        var existing = await _context.UserBranches
            .FirstOrDefaultAsync(ub => ub.UserId == userId && ub.BranchId == branchId, cancellationToken);

        if (existing != null)
        {
            existing.IsActive = true;
            existing.IsDefault = isDefault;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            _context.UserBranches.Add(new UserBranch
            {
                UserId = userId,
                BranchId = branchId,
                IsDefault = isDefault,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
        }

        if (isDefault)
        {
            var otherDefaults = await _context.UserBranches
                .Where(ub => ub.UserId == userId && ub.BranchId != branchId && ub.IsDefault)
                .ToListAsync(cancellationToken);

            foreach (var od in otherDefaults)
            {
                od.IsDefault = false;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> UnassignUserAsync(int userId, int branchId, CancellationToken cancellationToken = default)
    {
        var existing = await _context.UserBranches
            .FirstOrDefaultAsync(ub => ub.UserId == userId && ub.BranchId == branchId, cancellationToken);

        if (existing != null)
        {
            existing.IsActive = false;
            existing.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
        return false;
    }

    public async Task<IReadOnlyList<BranchPaymentMethod>> GetPaymentMethodsByBranchIdAsync(int branchId, CancellationToken cancellationToken = default)
    {
        return await _context.BranchPaymentMethods
            .AsNoTracking()
            .Include(bpm => bpm.PaymentMethod)
            .Where(bpm => bpm.BranchId == branchId && bpm.IsActive && bpm.PaymentMethod.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> SetPaymentMethodsAsync(int branchId, IEnumerable<int> paymentMethodIds, CancellationToken cancellationToken = default)
    {
        var current = await _context.BranchPaymentMethods
            .Where(bpm => bpm.BranchId == branchId)
            .ToListAsync(cancellationToken);

        _context.BranchPaymentMethods.RemoveRange(current);

        foreach (var pmId in paymentMethodIds)
        {
            _context.BranchPaymentMethods.Add(new BranchPaymentMethod
            {
                BranchId = branchId,
                PaymentMethodId = pmId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IReadOnlyList<User>> GetUsersByBranchIdAsync(int branchId, CancellationToken cancellationToken = default)
    {
        var branch = await _context.Branches.AsNoTracking().FirstOrDefaultAsync(b => b.Id == branchId, cancellationToken);
        var targetCompanyId = branch?.CompanyId;

        var branchUsers = await _context.UserBranches
            .AsNoTracking()
            .Where(ub => ub.BranchId == branchId && ub.IsActive && ub.User.IsActive)
            .Include(ub => ub.User)
                .ThenInclude(u => u.UserRoleIdNavigation)
            .Select(ub => ub.User)
            .ToListAsync(cancellationToken);

        var adminUsers = targetCompanyId.HasValue
            ? await _context.User
                .AsNoTracking()
                .Include(u => u.UserRoleIdNavigation)
                .Where(u => u.IsActive && u.CompanyId == targetCompanyId.Value && u.UserRoleIdNavigation != null && (u.UserRoleIdNavigation.Role == "Administrador" || u.UserRoleIdNavigation.Role == "Admin"))
                .ToListAsync(cancellationToken)
            : new List<User>();

        var combined = branchUsers.Concat(adminUsers)
            .GroupBy(u => u.Id)
            .Select(g => g.First())
            .OrderBy(u => u.FullName)
            .ToList();

        return combined;
    }
}
