namespace ParkingApi.Domain.Dtos.Auth;

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool MustChangePassword { get; set; }
    public int UserId { get; set; }
    public string? FullName { get; set; }
}
