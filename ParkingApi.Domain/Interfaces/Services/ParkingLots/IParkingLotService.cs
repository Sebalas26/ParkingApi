using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ParkingApi.Domain.Dtos.ParkingLots;

namespace ParkingApi.Domain.Interfaces.Services.ParkingLots;

public interface IParkingLotService
{
    Task<IReadOnlyList<ParkingLotDto>> GetParkingLotsAsync(CancellationToken cancellationToken = default);
    Task<ParkingLotDto?> GetParkingLotByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ParkingLotDto?> SaveOrEditParkingLotAsync(SaveParkingLotDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeactivateParkingLotAsync(int id, CancellationToken cancellationToken = default);
}
