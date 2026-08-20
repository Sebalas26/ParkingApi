using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Dtos.Actions;
using ParkingApi.Domain.Interfaces.Repositories.Actions;
using ParkingApi.Domain.Interfaces.Services.Actions;
using ParkingApi.Domain.Models;

namespace ParkingApi.Core.Services.Actions;

public class ActionService : IActionService
{
    private readonly IActionRepository _actionRepository;
    private readonly ILogger<ActionService> _logger;

    public ActionService(IActionRepository actionRepository, ILogger<ActionService> logger)
    {
        _actionRepository = actionRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<GetActionsDto>> GetActions(CancellationToken cancellation = default)
    {
        try
        {
            return await _actionRepository.GetActions(cancellation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener las acciones");
            return new List<GetActionsDto>();
        }
    }

    public async Task<IEnumerable<GetActionsDto>> GetActionsActive(CancellationToken cancellation = default)
    {
        try
        {
            return await _actionRepository.GetActionsActive(cancellation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener las acciones activas");
            return new List<GetActionsDto>();
        }
    }

    public async Task<GetActionsDto?> GetActionsById(int id, CancellationToken cancellation = default)
    {
        try
        {
            return await _actionRepository.GetActionsById(id, cancellation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener la acción por id {Id}", id);
            return null;
        }
    }

    public async Task<GetActionsDto> SaveOrEditActions(GetActionsDto getActions, CancellationToken cancellation = default)
    {
        var data = new GetActionsDto();
        try
        {
            var saveData = new Domain.Models.Action
            {
                Id = getActions.Id,
                Name = getActions.Name.Trim(),
                Slug = getActions.Slug.Trim(),
                ModuleId = getActions.Module.Id,
                OperationId = getActions.Operation.Id,
                IsActive = getActions.IsActive,
                CreatedAt = getActions.CreatedAt ?? DateTime.UtcNow
            };

            var isExist = await ValidateExist(getActions.Name, getActions.Module.Id, getActions.Operation.Id, cancellation);
            if (saveData.Id == 0 && !isExist)
            {
                data.CreatedAt = DateTime.UtcNow;
                await _actionRepository.SaveActions(saveData, cancellation);
            }
            else
            {
                data.UpdatedAt = DateTime.UtcNow;
                await _actionRepository.UpdateActions(saveData, cancellation);
            }

            data = await _actionRepository.GetActionByName(saveData.Name, saveData.ModuleId, saveData.OperationId, cancellation) ?? new();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al guardar o editar acciones");
        }
        return data;
    }

    private async Task<bool> ValidateExist(string name, int idModule, int idOperation, CancellationToken cancellation = default)
    {
        try
        {
            return await _actionRepository.GetActionByExist(name, idModule, idOperation, cancellation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al validar existencia de la acción");
            return false;
        }
    }
}
