using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ParkingApi.Domain.Models;

namespace ParkingApi.Domain.Interfaces.Repositories.Billing;

public interface IDianDocumentTypeRepository
{
    Task<IReadOnlyList<DianDocumentType>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DianDocumentType>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<DianDocumentType?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<DianDocumentType?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<DianDocumentType> AddAsync(DianDocumentType entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(DianDocumentType entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(DianDocumentType entity, CancellationToken cancellationToken = default);
}
