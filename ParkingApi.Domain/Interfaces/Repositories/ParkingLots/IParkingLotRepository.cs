using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ParkingApi.Domain.Models;

namespace ParkingApi.Domain.Interfaces.Repositories.ParkingLots;

public interface IParkingLotRepository
{
    Task<IReadOnlyList<ParkingLot>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ParkingLot?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ParkingLot> AddAsync(ParkingLot parkingLot, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(ParkingLot parkingLot, CancellationToken cancellationToken = default);
    Task SetEnrolledUsersAsync(int parkingLotId, List<int> userIds, CancellationToken cancellationToken = default);
    Task ClearMainImageFlagExceptAsync(int parkingLotId, CancellationToken cancellationToken = default);
}
