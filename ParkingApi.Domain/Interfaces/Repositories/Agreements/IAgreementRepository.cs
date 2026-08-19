using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ParkingApi.Domain.Models;

namespace ParkingApi.Domain.Interfaces.Repositories.Agreements;

public interface IAgreementRepository
{
    Task<IReadOnlyList<CommercialAgreement>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CommercialAgreement>> GetByStoreIdAsync(Guid storeId, CancellationToken cancellationToken = default);
    Task<CommercialAgreement?> GetByIdAsync(Guid agreementId, CancellationToken cancellationToken = default);
    Task<bool> AddAsync(CommercialAgreement agreement, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(CommercialAgreement agreement, CancellationToken cancellationToken = default);
}
