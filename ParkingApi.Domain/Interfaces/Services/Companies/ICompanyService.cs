using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ParkingApi.Domain.Dtos.Companies;

namespace ParkingApi.Domain.Interfaces.Services.Companies;

public interface ICompanyService
{
    Task<IReadOnlyList<CompanyDto>> GetAllCompaniesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CompanyDto>> GetActiveCompaniesAsync(CancellationToken cancellationToken = default);
    Task<CompanyDto?> GetCompanyByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<CompanyDto> CreateCompanyAsync(CreateCompanyDto dto, int? responsibleUserId = null, CancellationToken cancellationToken = default);
    Task<CompanyDto> UpdateCompanyAsync(int id, UpdateCompanyDto dto, int? responsibleUserId = null, CancellationToken cancellationToken = default);
    Task<bool> ToggleCompanyStatusAsync(int id, CancellationToken cancellationToken = default);
}
