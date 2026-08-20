using System.Threading;
using System.Threading.Tasks;
using ParkingApi.Domain.Dtos.Sync;

namespace ParkingApi.Domain.Interfaces.Services.Sync;

public interface ISyncService
{
    Task<BootstrapSyncDto> GetBootstrapDataAsync(CancellationToken cancellationToken = default);
}
