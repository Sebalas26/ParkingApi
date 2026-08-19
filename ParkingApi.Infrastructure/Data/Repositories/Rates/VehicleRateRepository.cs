using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ParkingApi.Domain.Common.Enums;
using ParkingApi.Domain.Interfaces.Repositories.Rates;
using ParkingApi.Domain.Models;
using ParkingApi.Infrastructure.Data;
using ParkingApi.Infrastructure.Data.Repositories.Base;

namespace ParkingApi.Infrastructure.Data.Repositories.Rates;

public class VehicleRateRepository : BaseRepository<VehicleRate>, IVehicleRateRepository
{
    public VehicleRateRepository(DataContext context) : base(context) { }

    public async Task<VehicleRate?> GetByTypeAsync(VehicleType type, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(r => r.VehicleType == type && r.IsActive, cancellationToken);
    }
}
