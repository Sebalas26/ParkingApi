using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Dtos.UserRoles;
using ParkingApi.Domain.Interfaces.Repositories.UserRoles;
using ParkingApi.Domain.Interfaces.Services.UserRoles;
using ParkingApi.Domain.Models;

namespace ParkingApi.Core.Services.UserRoles;

public class UserRoleService : IUserRoleService
{
    private readonly IUserRoleRepository _userRoleRepository;
    private readonly ILogger<UserRoleService> _logger;

    public UserRoleService(IUserRoleRepository userRoleRepository, ILogger<UserRoleService> logger)
    {
        _userRoleRepository = userRoleRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<GetUserRoleDto>> GetUserRoles(int? companyId = null, CancellationToken cancellation = default)
    {
        try
        {
            return await _userRoleRepository.GetUserRoles(companyId, cancellation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener los roles de los usuarios");
            return new List<GetUserRoleDto>();
        }
    }

    public async Task<GetUserRoleDto?> GetUserRoleById(int id, CancellationToken cancellation = default)
    {
        try
        {
            return await _userRoleRepository.GetUserRoleById(id, cancellation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener el rol del usuario con id {Id}", id);
            return null;
        }
    }

    public async Task<GetUserRoleDto> SaveOrEditUserRole(GetUserRoleDto userRole, CancellationToken cancellation = default)
    {
        var data = new GetUserRoleDto();
        try
        {
            var saveData = new UserRole
            {
                Id = userRole.IdUserRol,
                Role = userRole.RoleName.Trim(),
                IsActive = userRole.IsActive,
                CreatedAt = userRole.CreatedAt ?? DateTime.UtcNow
            };

            var isExist = await ValidateRole(userRole.RoleName, cancellation);
            if (saveData.Id == 0 && !isExist)
            {
                data.CreatedAt = DateTime.UtcNow;
                await _userRoleRepository.SaveUserRole(saveData, cancellation);
            }
            else
            {
                data.UpdatedAt = DateTime.UtcNow;
                await _userRoleRepository.UpdateUserRole(saveData, cancellation);
            }

            data = await _userRoleRepository.GetUserRoleName(saveData.Role, cancellation) ?? new();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al guardar o editar el rol del usuario");
        }
        return data;
    }

    public async Task<bool> DeleteUserRole(int id, CancellationToken cancellation = default)
    {
        try
        {
            return await _userRoleRepository.DeleteUserRole(id, cancellation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar el rol #{Id}", id);
            throw;
        }
    }

    private async Task<bool> ValidateRole(string roleName, CancellationToken cancellation = default)
    {
        try
        {
            var role = await _userRoleRepository.GetUserRoleName(roleName, cancellation);
            return role != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al validar el rol con nombre {Role}", roleName);
            return false;
        }
    }
}
