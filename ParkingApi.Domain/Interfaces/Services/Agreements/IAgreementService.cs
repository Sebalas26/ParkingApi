using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ParkingApi.Domain.Dtos.Agreements;

namespace ParkingApi.Domain.Interfaces.Services.Agreements;

public interface IAgreementService
{
    Task<IReadOnlyList<CommercialAgreementDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CommercialAgreementDto>> GetByStoreIdAsync(Guid storeId, CancellationToken cancellationToken = default);
    Task<CommercialAgreementDto?> GetByIdAsync(Guid agreementId, CancellationToken cancellationToken = default);
    Task<CommercialAgreementDto> CreateAsync(CreateAgreementDto dto, CancellationToken cancellationToken = default);
    Task<CommercialAgreementDto?> UpdateAsync(Guid agreementId, UpdateAgreementDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid agreementId, CancellationToken cancellationToken = default);
}
