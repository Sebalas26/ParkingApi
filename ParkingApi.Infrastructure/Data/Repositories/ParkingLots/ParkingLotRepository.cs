using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Interfaces.Repositories.ParkingLots;
using ParkingApi.Domain.Models;

namespace ParkingApi.Infrastructure.Data.Repositories.ParkingLots;

public class ParkingLotRepository : IParkingLotRepository
{
    private readonly DataContext _context;
    private readonly ILogger<ParkingLotRepository> _logger;

    public ParkingLotRepository(DataContext context, ILogger<ParkingLotRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ParkingLot>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.ParkingLots
                .AsNoTracking()
                .Include(p => p.UserParkings)
                    .ThenInclude(up => up.User)
                        .ThenInclude(u => u.UserRoleIdNavigation)
                .Include(p => p.UserParkings)
                    .ThenInclude(up => up.User)
                        .ThenInclude(u => u.IdentificationTypeIdNavigation)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar parqueaderos");
            return new List<ParkingLot>();
        }
    }

    public async Task<ParkingLot?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.ParkingLots
                .Include(p => p.UserParkings)
                    .ThenInclude(up => up.User)
                        .ThenInclude(u => u.UserRoleIdNavigation)
                .Include(p => p.UserParkings)
                    .ThenInclude(up => up.User)
                        .ThenInclude(u => u.IdentificationTypeIdNavigation)
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar parqueadero por id {Id}", id);
            return null;
        }
    }

    public async Task<ParkingLot> AddAsync(ParkingLot parkingLot, CancellationToken cancellationToken = default)
    {
        parkingLot.CreatedAt = DateTime.UtcNow;
        await _context.ParkingLots.AddAsync(parkingLot, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return parkingLot;
    }

    public async Task<bool> UpdateAsync(ParkingLot parkingLot, CancellationToken cancellationToken = default)
    {
        try
        {
            var existing = await _context.ParkingLots.FirstOrDefaultAsync(p => p.Id == parkingLot.Id, cancellationToken);
            if (existing == null) return false;

            existing.Name = parkingLot.Name;
            existing.Description = parkingLot.Description;
            existing.ImageUrl = parkingLot.ImageUrl;
            existing.IsMainImage = parkingLot.IsMainImage;
            existing.IsActive = parkingLot.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;

            return await _context.SaveChangesAsync(cancellationToken) > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar parqueadero {Id}", parkingLot.Id);
            return false;
        }
    }

    public async Task SetEnrolledUsersAsync(int parkingLotId, List<int> userIds, CancellationToken cancellationToken = default)
    {
        try
        {
            var existingRelations = await _context.UserParkings
                .Where(up => up.ParkingLotId == parkingLotId)
                .ToListAsync(cancellationToken);

            _context.UserParkings.RemoveRange(existingRelations);

            if (userIds != null && userIds.Any())
            {
                var newRelations = userIds.Distinct().Select(uId => new UserParking
                {
                    ParkingLotId = parkingLotId,
                    UserId = uId
                });

                await _context.UserParkings.AddRangeAsync(newRelations, cancellationToken);
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar usuarios enrolados para el parqueadero {Id}", parkingLotId);
        }
    }

    public async Task ClearMainImageFlagExceptAsync(int parkingLotId, CancellationToken cancellationToken = default)
    {
        try
        {
            var otherMainParkings = await _context.ParkingLots
                .Where(p => p.Id != parkingLotId && p.IsMainImage)
                .ToListAsync(cancellationToken);

            foreach (var p in otherMainParkings)
            {
                p.IsMainImage = false;
            }

            if (otherMainParkings.Any())
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al limpiar la bandera de imagen principal para otros parqueaderos");
        }
    }
}
