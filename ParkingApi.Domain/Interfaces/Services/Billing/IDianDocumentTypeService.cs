using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ParkingApi.Domain.Dtos.Billing;

namespace ParkingApi.Domain.Interfaces.Services.Billing;

public interface IDianDocumentTypeService
{
    Task<IReadOnlyList<DianDocumentTypeDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DianDocumentTypeDto>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<DianDocumentTypeDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<DianDocumentTypeDto> CreateAsync(CreateDianDocumentTypeDto dto, int? responsibleUserId = null, CancellationToken cancellationToken = default);
    Task<DianDocumentTypeDto> UpdateAsync(int id, UpdateDianDocumentTypeDto dto, int? responsibleUserId = null, CancellationToken cancellationToken = default);
    Task<bool> ToggleStatusAsync(int id, int? responsibleUserId = null, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
