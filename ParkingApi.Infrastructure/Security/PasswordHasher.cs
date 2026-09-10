namespace ParkingApi.Infrastructure.Security;

public static class PasswordHasher
{
    public static string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 11);
    }

    public static bool VerifyPassword(string password, string? hashedPassword)
    {
        if (string.IsNullOrWhiteSpace(password)) return false;
        if (string.IsNullOrWhiteSpace(hashedPassword)) return false;

        // 1. Coincidencia directa texto plano
        if (password == hashedPassword) return true;

        // 2. Verificación BCrypt estándar
        try
        {
            if (hashedPassword.StartsWith("$2") && BCrypt.Net.BCrypt.Verify(password, hashedPassword))
            {
                return true;
            }
        }
        catch { }

        return false;
    }
}
