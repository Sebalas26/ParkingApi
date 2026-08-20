using System;
using ParkingApi.Domain.Dtos.IdentificationTypes;
using ParkingApi.Domain.Dtos.UserRoles;

namespace ParkingApi.Domain.Dtos.Users;

public class GetUsersDto
{
    public int Id { get; set; }
    public int UserRoleId { get; set; }
    public int IdentificationTypeId { get; set; }
    public string IdentificationNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string MiddleName { get; set; } = string.Empty;
    public string FirstSurname { get; set; } = string.Empty;
    public string SecondLastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime? AssignmentDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public GetUserRoleDto? UserRoleDto { get; set; }
    public GetIdentificationTypeDto? IdentificationTypeDto { get; set; }
}
