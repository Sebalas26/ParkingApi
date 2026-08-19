using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ParkingApi.Domain.Interfaces.Repositories.Agreements;
using ParkingApi.Domain.Models;
using ParkingApi.Infrastructure.Data;
using ParkingApi.Infrastructure.Data.Repositories.Base;

namespace ParkingApi.Infrastructure.Data.Repositories.Agreements;

public class AgreementRepository : BaseRepository<CommercialAgreement>, IAgreementRepository
{
    public AgreementRepository(DataContext context) : base(context) { }

    public async Task<IReadOnlyList<CommercialAgreement>> GetByStoreIdAsync(Guid storeId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(a => a.StoreId == storeId && a.IsActive)
            .ToListAsync(cancellationToken);
    }
}
