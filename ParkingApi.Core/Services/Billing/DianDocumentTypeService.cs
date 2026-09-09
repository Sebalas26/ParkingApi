using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Dtos.Billing;
using ParkingApi.Domain.Interfaces.Repositories.Billing;
using ParkingApi.Domain.Interfaces.Services.Billing;
using ParkingApi.Domain.Models;

namespace ParkingApi.Core.Services.Billing;

public class DianDocumentTypeService : IDianDocumentTypeService
{
    private readonly IDianDocumentTypeRepository _repository;
    private readonly ILogger<DianDocumentTypeService> _logger;

    public DianDocumentTypeService(
        IDianDocumentTypeRepository repository,
        ILogger<DianDocumentTypeService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IReadOnlyList<DianDocumentTypeDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var list = await _repository.GetAllAsync(cancellationToken);
        return list.Select(MapToDto).ToList();
    }

    public async Task<IReadOnlyList<DianDocumentTypeDto>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        var list = await _repository.GetActiveAsync(cancellationToken);
        return list.Select(MapToDto).ToList();
    }

    public async Task<DianDocumentTypeDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        return entity != null ? MapToDto(entity) : null;
    }

    public async Task<DianDocumentTypeDto> CreateAsync(CreateDianDocumentTypeDto dto, int? responsibleUserId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new InvalidOperationException("El nombre del tipo de documento es obligatorio.");

        if (string.IsNullOrWhiteSpace(dto.DefaultPrefix))
            throw new InvalidOperationException("El prefijo por defecto es obligatorio.");

        var trimmedName = dto.Name.Trim();
        var trimmedCode = string.IsNullOrWhiteSpace(dto.Code) ? trimmedName : dto.Code.Trim();

        var existing = await _repository.GetByCodeAsync(trimmedCode, cancellationToken);
        if (existing != null)
            throw new InvalidOperationException($"Ya existe un tipo de documento configurado con el código '{trimmedCode}'.");

        var entity = new DianDocumentType
        {
            Name = trimmedName,
            Code = trimmedCode,
            DefaultPrefix = dto.DefaultPrefix.Trim().ToUpperInvariant(),
            Description = dto.Description?.Trim(),
            RequiresTechnicalKey = dto.RequiresTechnicalKey,
            IsActive = true,
            ResponsibleUserId = responsibleUserId,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repository.AddAsync(entity, cancellationToken);
        _logger.LogInformation("Tipo de documento DIAN creado: {Name} ({Code})", created.Name, created.Code);
        return MapToDto(created);
    }

    public async Task<DianDocumentTypeDto> UpdateAsync(int id, UpdateDianDocumentTypeDto dto, int? responsibleUserId = null, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity == null)
            throw new KeyNotFoundException($"Tipo de documento DIAN con ID {id} no encontrado.");

        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new InvalidOperationException("El nombre del tipo de documento es obligatorio.");

        if (string.IsNullOrWhiteSpace(dto.DefaultPrefix))
            throw new InvalidOperationException("El prefijo por defecto es obligatorio.");

        entity.Name = dto.Name.Trim();
        entity.Code = string.IsNullOrWhiteSpace(dto.Code) ? entity.Name : dto.Code.Trim();
        entity.DefaultPrefix = dto.DefaultPrefix.Trim().ToUpperInvariant();
        entity.Description = dto.Description?.Trim();
        entity.RequiresTechnicalKey = dto.RequiresTechnicalKey;
        entity.IsActive = dto.IsActive;
        entity.ResponsibleUserId = responsibleUserId;
        entity.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(entity, cancellationToken);
        _logger.LogInformation("Tipo de documento DIAN actualizado: {Id} - {Name}", id, entity.Name);
        return MapToDto(entity);
    }

    public async Task<bool> ToggleStatusAsync(int id, int? responsibleUserId = null, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity == null)
            throw new KeyNotFoundException($"Tipo de documento DIAN con ID {id} no encontrado.");

        entity.IsActive = !entity.IsActive;
        entity.ResponsibleUserId = responsibleUserId;
        entity.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(entity, cancellationToken);
        return entity.IsActive;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity == null)
            throw new KeyNotFoundException($"Tipo de documento DIAN con ID {id} no encontrado.");

        await _repository.DeleteAsync(entity, cancellationToken);
        _logger.LogInformation("Tipo de documento DIAN eliminado: {Id}", id);
        return true;
    }

    private static DianDocumentTypeDto MapToDto(DianDocumentType d)
    {
        return new DianDocumentTypeDto
        {
            Id = d.Id,
            Name = d.Name,
            Code = d.Code,
            DefaultPrefix = d.DefaultPrefix,
            Description = d.Description,
            RequiresTechnicalKey = d.RequiresTechnicalKey,
            IsActive = d.IsActive,
            CreatedAt = d.CreatedAt
        };
    }
}
