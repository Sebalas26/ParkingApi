using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ParkingApi.Domain.Dtos.Operations;

namespace ParkingApi.Domain.Interfaces.Services.Operations;

public interface IOperationService
{
    Task<IEnumerable<GetOperationDto>> GetOperations(CancellationToken cancellation = default);
    Task<GetOperationDto?> GetOperationsById(int id, CancellationToken cancellation = default);
    Task<GetOperationDto> SaveOrEditOperation(GetOperationDto operation, CancellationToken cancellation = default);
}
