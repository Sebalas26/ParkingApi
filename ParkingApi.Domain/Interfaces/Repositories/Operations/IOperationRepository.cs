using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ParkingApi.Domain.Dtos.Operations;
using ParkingApi.Domain.Models;

namespace ParkingApi.Domain.Interfaces.Repositories.Operations;

public interface IOperationRepository
{
    Task<IEnumerable<GetOperationDto>> GetOperations(CancellationToken cancellation = default);
    Task<GetOperationDto?> GetOperationsById(int id, CancellationToken cancellation = default);
    Task<GetOperationDto?> GetOperationName(string operationName, CancellationToken cancellation = default);
    Task<bool> SaveOperation(Operation operation, CancellationToken cancellation = default);
    Task<bool> UpdateOperation(Operation operation, CancellationToken cancellation = default);
}
