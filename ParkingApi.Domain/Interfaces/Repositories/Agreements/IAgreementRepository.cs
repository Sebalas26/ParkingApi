using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ParkingApi.Domain.Interfaces.Repositories.Base;
using ParkingApi.Domain.Models;

namespace ParkingApi.Domain.Interfaces.Repositories.Agreements;

public interface IAgreementRepository : IBaseRepository<CommercialAgreement>
{
    Task<IReadOnlyList<CommercialAgreement>> GetByStoreIdAsync(Guid storeId, CancellationToken cancellationToken = default);
}
