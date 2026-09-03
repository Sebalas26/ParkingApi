using System;
using System.Collections.Generic;
using ParkingApi.Domain.Dtos.Branches;

namespace ParkingApi.Domain.Dtos.Auth;

public class AuthResponseDto
{
    public bool Success { get; set; }
    public string? Token { get; set; }
    public string? ErrorMessage { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public bool IsAdmin { get; set; }
    public bool IsSuperAdmin { get; set; }
    public int? CompanyId { get; set; }
    public string? CompanyName { get; set; }
    public int? MaxBranches { get; set; }
    public bool AllowMultipleSessions { get; set; }
    public int MaxActiveSessionsPerUser { get; set; } = 1;
    public bool RequireOpenShiftToOperate { get; set; } = true;
    public bool AllowMultipleOpenShifts { get; set; }
    public int MaxOpenShiftsPerUser { get; set; } = 1;
    public bool RequireInitialCashAmount { get; set; } = true;
    public List<BranchDto> Branches { get; set; } = new();
    public List<string> Permissions { get; set; } = new();
}
