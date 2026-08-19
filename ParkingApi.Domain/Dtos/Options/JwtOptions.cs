namespace ParkingApi.Domain.Dtos.Options;

public class JwtOptions
{
    public string JwtSigningKey { get; set; } = "PARKFLOW_ENTERPRISE_SECRET_KEY_2026_JWT_SUPER_SECURE_TOKEN_123456789";
    public string Issuer { get; set; } = "ParkFlowApi";
    public string Audience { get; set; } = "ParkFlowClients";
    public int AccessTokenMinutes { get; set; } = 1440;
}
