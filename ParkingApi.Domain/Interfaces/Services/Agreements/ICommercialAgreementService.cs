using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ParkingApi.Domain.Models;

namespace ParkingApi.Domain.Interfaces.Services.Agreements;

public interface ICommercialAgreementService
{
    Task<IReadOnlyList<CommercialAgreement>> GetAllAsync(int? companyId = null, CancellationToken cancellationToken = default);
    Task<CommercialAgreement?> GetByIdAsync(Guid agreementId, CancellationToken cancellationToken = default);
    Task<CommercialAgreement> CreateAsync(CommercialAgreement agreement, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(CommercialAgreement agreement, CancellationToken cancellationToken = default);
}
