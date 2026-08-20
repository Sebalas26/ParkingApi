using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Dtos.Users;
using ParkingApi.Domain.Interfaces.Repositories.Login;
using ParkingApi.Domain.Interfaces.Services.Login;
using ParkingApi.Domain.Models;

namespace ParkingApi.Core.Services.Login;

public class LoginService : ILoginService
{
    private readonly ILoginRepository _loginRepository;
    private readonly ILogger<LoginService> _logger;

    public LoginService(ILoginRepository loginRepository, ILogger<LoginService> logger)
    {
        _loginRepository = loginRepository;
        _logger = logger;
    }

    public async Task<bool> AddUserLogin(LoginUserDto user, CancellationToken cancellation = default)
    {
        try
        {
            var login = new Domain.Models.Login
            {
                UserId = user.Id,
                Message = $"Inicio de sesión exitoso para usuario {user.UserName}",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            return await _loginRepository.AddUserLogin(login, cancellation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al registrar login");
            return false;
        }
    }
}
