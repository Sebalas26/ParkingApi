namespace ParkingApi.Domain.Interfaces.Services;

public interface ICurrentUserService
{
    string? UserId { get; }
    int? ParsedUserId { get; }
    int? CompanyId { get; }
    bool IsSuperAdmin { get; }
    int? RoleId { get; }
    string? RoleName { get; }
    int? GetEffectiveCompanyId(int? requestedCompanyId);
    bool CanAccessCompany(int targetCompanyId);
}

