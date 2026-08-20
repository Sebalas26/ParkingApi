using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Common.Constants;
using ParkingApi.Domain.Dtos.PaymentMethods;
using ParkingApi.Domain.Interfaces.Repositories.PaymentMethods;
using ParkingApi.Domain.Interfaces.Services;
using ParkingApi.Domain.Models;

namespace ParkingApi.Infrastructure.Data.Repositories.PaymentMethods;

public class PaymentMethodRepository : IPaymentMethodRepository
{
    private readonly DataContext _context;
    private readonly ILogger<PaymentMethodRepository> _logger;
    private readonly ICurrentUserService _currentUser;

    public PaymentMethodRepository(DataContext context, ILogger<PaymentMethodRepository> logger, ICurrentUserService currentUser)
    {
        _context = context;
        _logger = logger;
        _currentUser = currentUser;
    }

    public async Task<IEnumerable<GetPaymentMethodDto>> GetAllAsync(CancellationToken cancellation = default)
    {
        try
        {
            return await _context.PaymentMethod
                .AsNoTracking()
                .Select(x => new GetPaymentMethodDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Icon = x.Icon,
                    IsActive = x.IsActive,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt
                })
                .OrderBy(x => x.Name)
                .ToListAsync(cancellation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, Constants.PaymentMethodError);
            return Enumerable.Empty<GetPaymentMethodDto>();
        }
    }

    public async Task<IEnumerable<GetPaymentMethodDto>> GetAllActiveAsync(CancellationToken cancellation = default)
    {
        try
        {
            return await _context.PaymentMethod
                .AsNoTracking()
                .Where(x => x.IsActive)
                .Select(x => new GetPaymentMethodDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Icon = x.Icon,
                    IsActive = x.IsActive,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt
                })
                .OrderBy(x => x.Name)
                .ToListAsync(cancellation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, Constants.PaymentMethodError);
            return Enumerable.Empty<GetPaymentMethodDto>();
        }
    }

    public async Task<GetPaymentMethodDto?> GetByIdAsync(int id, CancellationToken cancellation = default)
    {
        try
        {
            return await _context.PaymentMethod
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new GetPaymentMethodDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Icon = x.Icon,
                    IsActive = x.IsActive,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt
                })
                .FirstOrDefaultAsync(cancellation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, Constants.PaymentMethodError);
            return null;
        }
    }

    public async Task<bool> CreateAsync(PaymentMethod paymentMethod, CancellationToken cancellation = default)
    {
        try
        {
            if (int.TryParse(_currentUser?.UserId, out int uid))
            {
                paymentMethod.ResponsibleUserId = uid;
            }
            paymentMethod.CreatedAt = DateTime.UtcNow;
            await _context.PaymentMethod.AddAsync(paymentMethod, cancellation);
            return await _context.SaveChangesAsync(cancellation) > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear método de pago");
            return false;
        }
    }

    public async Task<bool> UpdateAsync(PaymentMethod paymentMethod, CancellationToken cancellation = default)
    {
        try
        {
            var existing = await _context.PaymentMethod.FirstOrDefaultAsync(p => p.Id == paymentMethod.Id, cancellation);
            if (existing == null) return false;

            if (int.TryParse(_currentUser?.UserId, out int uid))
            {
                existing.ResponsibleUserId = uid;
            }
            existing.Name = paymentMethod.Name;
            existing.Icon = paymentMethod.Icon;
            existing.IsActive = paymentMethod.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;

            return await _context.SaveChangesAsync(cancellation) > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar método de pago");
            return false;
        }
    }

    public async Task<GetPaymentMethodDto?> ValidateExist(string name, CancellationToken cancellation = default)
    {
        try
        {
            var normalized = name.Trim().ToLower();
            return await _context.PaymentMethod
                .AsNoTracking()
                .Where(x => x.Name.ToLower() == normalized)
                .Select(x => new GetPaymentMethodDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Icon = x.Icon,
                    IsActive = x.IsActive,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt
                })
                .FirstOrDefaultAsync(cancellation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, Constants.PaymentMethodError);
            return null;
        }
    }
}
