using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ParkingApi.Domain.Models;

namespace ParkingApi.Domain.Interfaces.Repositories.Companies;

public interface ICompanyRepository
{
    Task<IReadOnlyList<Company>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Company>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<Company?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Company?> GetByNitAsync(string nit, CancellationToken cancellationToken = default);
    Task<Company> AddAsync(Company company, CancellationToken cancellationToken = default);
    Task<Company> UpdateAsync(Company company, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
