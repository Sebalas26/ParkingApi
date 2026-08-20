namespace ParkingApi.Domain.Dtos.Auth;

public class IncomeDto
{
    public string Fullname { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public bool Success { get; set; }
    public int IdUser { get; set; }
    public int IdRoleUser { get; set; }
}
