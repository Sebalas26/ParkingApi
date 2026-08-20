using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Interfaces.Repositories.Login;
using ParkingApi.Domain.Models;

namespace ParkingApi.Infrastructure.Data.Repositories.Login;

public class LoginRepository : ILoginRepository
{
    private readonly DataContext _context;
    private readonly ILogger<LoginRepository> _logger;

    public LoginRepository(DataContext context, ILogger<LoginRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> AddUserLogin(Domain.Models.Login login, CancellationToken cancellation = default)
    {
        try
        {
            login.CreatedAt = DateTime.UtcNow;
            await _context.Login.AddAsync(login, cancellation);
            return await _context.SaveChangesAsync(cancellation) > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al registrar bitácora de login");
            return false;
        }
    }
}
