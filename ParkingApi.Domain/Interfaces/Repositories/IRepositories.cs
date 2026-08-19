using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace ParkingApi.Domain.Interfaces.Repositories;

public interface IBaseRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IUserRepository : IBaseRepository<Models.User>
{
    Task<Models.User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
}

public interface IParkingTicketRepository : IBaseRepository<Models.ParkingTicket>
{
    Task<Models.ParkingTicket?> GetActiveByPlateAsync(string plateNumber, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Models.ParkingTicket>> GetActiveTicketsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Models.ParkingTicket>> GetTodayCompletedTicketsAsync(CancellationToken cancellationToken = default);
}

public interface IVehicleRateRepository : IBaseRepository<Models.VehicleRate>
{
    Task<Models.VehicleRate?> GetByTypeAsync(Common.Enums.VehicleType type, CancellationToken cancellationToken = default);
}

public interface IStoreRepository : IBaseRepository<Models.Store> { }
public interface IAgreementRepository : IBaseRepository<Models.CommercialAgreement> { }
public interface IUserSessionRepository : IBaseRepository<Models.UserSession> { }
