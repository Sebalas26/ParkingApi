using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Dtos.Operations;
using ParkingApi.Domain.Interfaces.Repositories.Operations;
using ParkingApi.Domain.Interfaces.Services.Operations;
using ParkingApi.Domain.Models;

namespace ParkingApi.Core.Services.Operations;

public class OperationService : IOperationService
{
    private readonly IOperationRepository _operationRepository;
    private readonly ILogger<OperationService> _logger;

    public OperationService(IOperationRepository operationRepository, ILogger<OperationService> logger)
    {
        _operationRepository = operationRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<GetOperationDto>> GetOperations(CancellationToken cancellation = default)
    {
        try
        {
            return await _operationRepository.GetOperations(cancellation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener las operaciones");
            return new List<GetOperationDto>();
        }
    }

    public async Task<GetOperationDto?> GetOperationsById(int id, CancellationToken cancellation = default)
    {
        try
        {
            return await _operationRepository.GetOperationsById(id, cancellation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener la operación por id {Id}", id);
            return null;
        }
    }

    public async Task<GetOperationDto> SaveOrEditOperation(GetOperationDto operation, CancellationToken cancellation = default)
    {
        var data = new GetOperationDto();
        try
        {
            var saveData = new Operation
            {
                Id = operation.Id,
                Name = operation.Name.Trim(),
                IsActive = operation.IsActive,
                CreatedAt = operation.CreatedAt ?? DateTime.UtcNow
            };

            var isExist = await ValidateOperation(operation.Name, cancellation);
            if (saveData.Id == 0 && !isExist)
            {
                data.CreatedAt = DateTime.UtcNow;
                await _operationRepository.SaveOperation(saveData, cancellation);
            }
            else
            {
                data.UpdatedAt = DateTime.UtcNow;
                await _operationRepository.UpdateOperation(saveData, cancellation);
            }

            data = await _operationRepository.GetOperationName(saveData.Name, cancellation) ?? new();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al guardar o editar la operación");
        }
        return data;
    }

    private async Task<bool> ValidateOperation(string name, CancellationToken cancellation = default)
    {
        try
        {
            var operation = await _operationRepository.GetOperationName(name, cancellation);
            return operation != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al validar la operación con nombre {Name}", name);
            return false;
        }
    }
}
