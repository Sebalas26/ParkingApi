using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Common.Constants;
using ParkingApi.Domain.Dtos.IdentificationTypes;
using ParkingApi.Domain.Interfaces.Repositories.IdentificationTypes;
using ParkingApi.Domain.Interfaces.Services.IdentificationTypes;
using ParkingApi.Domain.Models;

namespace ParkingApi.Core.Services.IdentificationTypes;

public class IdentificationTypeService : IIdentificationTypeService
{
    private readonly IIdentificationTypeRepository _repository;
    private readonly ILogger<IdentificationTypeService> _logger;

    public IdentificationTypeService(IIdentificationTypeRepository repository, ILogger<IdentificationTypeService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IEnumerable<GetIdentificationTypeDto>> GetAllAsync(CancellationToken cancellation = default)
    {
        try
        {
            return await _repository.GetAllAsync(cancellation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, Constants.IdentificationTypeError);
            return new List<GetIdentificationTypeDto>();
        }
    }

    public async Task<IEnumerable<GetIdentificationTypeDto>> GetAllActiveAsync(CancellationToken cancellation = default)
    {
        try
        {
            return await _repository.GetAllActiveAsync(cancellation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, Constants.IdentificationTypeError);
            return new List<GetIdentificationTypeDto>();
        }
    }

    public async Task<GetIdentificationTypeDto?> GetByIdAsync(int id, CancellationToken cancellation = default)
    {
        try
        {
            return await _repository.GetByIdAsync(id, cancellation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Error}: Error al obtener tipo de identificación por id {Id}", Constants.IdentificationTypeError, id);
            return null;
        }
    }

    public async Task<GetIdentificationTypeDto> CreateOrEditIdentificationType(GetIdentificationTypeDto identificationType, CancellationToken cancellation = default)
    {
        var result = new GetIdentificationTypeDto();
        try
        {
            var data = new IdentificationType
            {
                Id = identificationType.Id,
                Identification = identificationType.Name.Trim(),
                IsActive = identificationType.IsActive
            };

            var isExist = await _repository.ValidateExist(identificationType.Name, cancellation);
            if (data.Id == 0 && !isExist)
            {
                data.CreatedAt = DateTime.UtcNow;
                await _repository.CreateAsync(data, cancellation);
            }
            else
            {
                data.UpdatedAt = DateTime.UtcNow;
                await _repository.UpdateAsync(data, cancellation);
            }

            result = await _repository.GetByNameAsync(data.Identification, cancellation) ?? new();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Error}: Error al crear o editar tipo de identificación", Constants.IdentificationTypeError);
        }
        return result;
    }
}
