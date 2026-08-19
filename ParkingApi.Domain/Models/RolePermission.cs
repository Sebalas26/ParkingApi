using System;

namespace ParkingApi.Domain.Models;

public class RolePermission
{
    public Guid RolePermissionId { get; set; } = Guid.NewGuid();
    public Guid RoleId { get; set; }
    public Guid ModuleId { get; set; }
    public string PermissionSlug { get; set; } = string.Empty;
    public bool CanView { get; set; } = true;
    public bool CanCreate { get; set; } = true;
    public bool CanEdit { get; set; } = true;
    public bool CanDelete { get; set; } = true;

    public Role Role { get; set; } = null!;
    public AppModule Module { get; set; } = null!;
}
