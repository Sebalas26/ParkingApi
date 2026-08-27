using System;
using System.Collections.Generic;

namespace ParkingApi.Domain.Models;

public class User
{
    public int Id { get; set; }
    public int? CompanyId { get; set; }
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
    public string? Token { get; set; }
    public DateTime? AssignmentDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public bool IsActive { get; set; } = true;
    public bool MustChangePassword { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public virtual Company? Company { get; set; }
    public virtual UserRole UserRoleIdNavigation { get; set; } = null!;
    public virtual IdentificationType IdentificationTypeIdNavigation { get; set; } = null!;
    public virtual ICollection<Login> Logins { get; set; } = new List<Login>();
    public virtual ICollection<PasswordResetToken> PasswordResetTokens { get; set; } = new List<PasswordResetToken>();
    public virtual ICollection<UserBranch> UserBranches { get; set; } = new List<UserBranch>();
}
