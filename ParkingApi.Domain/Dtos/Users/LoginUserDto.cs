namespace ParkingApi.Domain.Dtos.Users;

public class LoginUserDto
{
    public int Id { get; set; }
    public string Fullname { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? Token { get; set; }
    public int? IdUserRole { get; set; }
    public int? ExpireToken { get; set; }
}
