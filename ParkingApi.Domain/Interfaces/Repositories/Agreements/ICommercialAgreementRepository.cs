using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ParkingApi.Domain.Models;

namespace ParkingApi.Domain.Interfaces.Repositories.Agreements;

public interface ICommercialAgreementRepository
{
    Task<IReadOnlyList<CommercialAgreement>> GetAllAsync(int? companyId = null, CancellationToken cancellationToken = default);
    Task<CommercialAgreement?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CommercialAgreement>> GetByStoreIdAsync(Guid storeId, CancellationToken cancellationToken = default);
    Task<CommercialAgreement> AddAsync(CommercialAgreement agreement, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(CommercialAgreement agreement, CancellationToken cancellationToken = default);
}
