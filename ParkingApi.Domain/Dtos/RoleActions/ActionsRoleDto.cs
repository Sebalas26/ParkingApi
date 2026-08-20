namespace ParkingApi.Domain.Dtos.RoleActions;

public class ActionsRoleDto
{
    public int ActionId { get; set; }
    public int ModuleId { get; set; }
    public bool IsActive { get; set; }
    public string? ActionName { get; set; }
}

public class ValidateRolActionDto
{
    public int Id { get; set; }
    public int ActionId { get; set; }
}

public class SaveRoleActionDto
{
    public int RoleId { get; set; }
    public int ActionId { get; set; }
    public bool IsActive { get; set; } = true;
}
