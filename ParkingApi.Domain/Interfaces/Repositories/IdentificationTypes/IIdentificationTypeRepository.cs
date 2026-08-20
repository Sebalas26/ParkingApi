using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ParkingApi.Domain.Dtos.IdentificationTypes;
using ParkingApi.Domain.Models;

namespace ParkingApi.Domain.Interfaces.Repositories.IdentificationTypes;

public interface IIdentificationTypeRepository
{
    Task<IEnumerable<GetIdentificationTypeDto>> GetAllAsync(CancellationToken cancellation = default);
    Task<IEnumerable<GetIdentificationTypeDto>> GetAllActiveAsync(CancellationToken cancellation = default);
    Task<GetIdentificationTypeDto?> GetByIdAsync(int id, CancellationToken cancellation = default);
    Task<GetIdentificationTypeDto?> GetByNameAsync(string name, CancellationToken cancellation = default);
    Task<bool> CreateAsync(IdentificationType identificationType, CancellationToken cancellation = default);
    Task<bool> UpdateAsync(IdentificationType identificationType, CancellationToken cancellation = default);
    Task<bool> ValidateExist(string name, CancellationToken cancellation = default);
}
