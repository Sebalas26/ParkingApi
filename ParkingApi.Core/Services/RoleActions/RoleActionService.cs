using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Dtos.RoleActions;
using ParkingApi.Domain.Interfaces.Repositories.RoleActions;
using ParkingApi.Domain.Interfaces.Services.RoleActions;

namespace ParkingApi.Core.Services.RoleActions;

public class RoleActionService : IRoleActionService
{
    private readonly IRoleActionRepository _roleActionsRepository;
    private readonly ILogger<RoleActionService> _logger;

    public RoleActionService(IRoleActionRepository roleActionsRepository, ILogger<RoleActionService> logger)
    {
        _roleActionsRepository = roleActionsRepository;
        _logger = logger;
    }

    public async Task<List<ActionsRoleDto>> GetActionsByRoleAsync(int roleId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _roleActionsRepository.GetActionsByRoleAsync(roleId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar las acciones por rol {RoleId}", roleId);
            return new List<ActionsRoleDto>();
        }
    }

    public async Task<List<string>> GetActionsByRoleIdAsync(int roleId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _roleActionsRepository.GetActionsByRoleIdAsync(roleId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error consultando las acciones del rol {RoleId}", roleId);
            return new List<string>();
        }
    }

    public async Task<bool> AssignRolePermissionsAsync(int roleId, List<int> actionIds, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _roleActionsRepository.AssignRolePermissionsAsync(roleId, actionIds, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error asignando permisos al rol {RoleId}", roleId);
            return false;
        }
    }
}
