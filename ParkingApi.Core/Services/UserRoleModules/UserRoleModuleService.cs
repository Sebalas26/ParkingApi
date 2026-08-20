using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Dtos.RoleActions;
using ParkingApi.Domain.Dtos.UserRoleModules;
using ParkingApi.Domain.Interfaces.Repositories.RoleActions;
using ParkingApi.Domain.Interfaces.Repositories.UserRoleModules;
using ParkingApi.Domain.Interfaces.Services.UserRoleModules;
using ParkingApi.Domain.Models;

namespace ParkingApi.Core.Services.UserRoleModules;

public class UserRoleModuleService : IUserRoleModuleService
{
    private readonly IUserRoleModuleRepository _userRoleModuleRepository;
    private readonly IRoleActionRepository _roleActionRepository;
    private readonly ILogger<UserRoleModuleService> _logger;

    public UserRoleModuleService(
        IUserRoleModuleRepository userRoleModuleRepository,
        IRoleActionRepository roleActionRepository,
        ILogger<UserRoleModuleService> logger)
    {
        _userRoleModuleRepository = userRoleModuleRepository;
        _roleActionRepository = roleActionRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<GetUserRoleModuleDto>> GetUserRoleModules(CancellationToken cancellation = default)
    {
        try
        {
            return await _userRoleModuleRepository.GetUserRoleModules(cancellation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar los módulos de rol de usuario");
            return new List<GetUserRoleModuleDto>();
        }
    }

    public async Task<GetUserRoleModuleDto?> GetUserRoleModuleById(int id, CancellationToken cancellation = default)
    {
        try
        {
            return await _userRoleModuleRepository.GetUserRoleModuleById(id, cancellation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar el módulo de rol de usuario por id {Id}", id);
            return null;
        }
    }

    public async Task<GetUserRoleModuleDto> SaveOrEditUserRoleModule(SaveUserRoleModuleDto saveUserRoleModule, CancellationToken cancellation = default)
    {
        var result = new GetUserRoleModuleDto();
        try
        {
            var userRoleModule = new UserRoleModule
            {
                ModulesRoleId = saveUserRoleModule.ModulesRoleId,
                UserRoleId = saveUserRoleModule.UserRoleId,
                IsActive = saveUserRoleModule.IsActive
            };

            var isExist = await _userRoleModuleRepository.ValidateExistUserRoleModule(saveUserRoleModule.UserRoleId, saveUserRoleModule.ModulesRoleId, cancellation);
            if (!isExist)
            {
                userRoleModule.CreatedAt = DateTime.UtcNow;
                await _userRoleModuleRepository.SaveUserRoleModule(userRoleModule, cancellation);
            }
            else
            {
                userRoleModule.UpdatedAt = DateTime.UtcNow;
                await _userRoleModuleRepository.UpdateUserRoleModule(userRoleModule, cancellation);
            }

            var actionsSaved = await _roleActionRepository.ValidateActionRoleAsync(saveUserRoleModule.UserRoleId, cancellation);

            foreach (var item in saveUserRoleModule.Actions)
            {
                var roleAction = new RoleAction
                {
                    ActionId = item.ActionId,
                    RoleId = saveUserRoleModule.UserRoleId,
                    IsActive = item.IsActive
                };

                if (actionsSaved.Count == 0 || !actionsSaved.Exists(x => x.ActionId == item.ActionId))
                {
                    roleAction.CreatedAt = DateTime.UtcNow;
                    await _roleActionRepository.SaveRoleAction(roleAction, cancellation);
                }
                else
                {
                    var actionExisting = actionsSaved.Find(x => x.ActionId == item.ActionId);
                    if (actionExisting != null)
                    {
                        roleAction.Id = actionExisting.Id;
                        roleAction.UpdatedAt = DateTime.UtcNow;
                        await _roleActionRepository.ActiveOrInactiveRoleAction(roleAction, cancellation);
                    }
                }
            }

            result = await _userRoleModuleRepository.GetuserRoleModulesCreate(saveUserRoleModule.UserRoleId, saveUserRoleModule.ModulesRoleId, cancellation) ?? new();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al guardar o editar el módulo de rol de usuario");
        }
        return result;
    }
}
