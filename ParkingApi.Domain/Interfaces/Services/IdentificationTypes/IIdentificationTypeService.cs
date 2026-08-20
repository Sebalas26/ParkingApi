using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ParkingApi.Domain.Dtos.IdentificationTypes;

namespace ParkingApi.Domain.Interfaces.Services.IdentificationTypes;

public interface IIdentificationTypeService
{
    Task<IEnumerable<GetIdentificationTypeDto>> GetAllAsync(CancellationToken cancellation = default);
    Task<IEnumerable<GetIdentificationTypeDto>> GetAllActiveAsync(CancellationToken cancellation = default);
    Task<GetIdentificationTypeDto?> GetByIdAsync(int id, CancellationToken cancellation = default);
    Task<GetIdentificationTypeDto> CreateOrEditIdentificationType(GetIdentificationTypeDto identificationType, CancellationToken cancellation = default);
}
