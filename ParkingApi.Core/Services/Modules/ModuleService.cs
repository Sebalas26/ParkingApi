using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Dtos.Modules;
using ParkingApi.Domain.Interfaces.Repositories.Modules;
using ParkingApi.Domain.Interfaces.Services.Modules;
using ParkingApi.Domain.Models;

namespace ParkingApi.Core.Services.Modules;

public class ModuleService : IModuleService
{
    private readonly IModuleRepository _moduleRepository;
    private readonly ILogger<ModuleService> _logger;

    public ModuleService(IModuleRepository moduleRepository, ILogger<ModuleService> logger)
    {
        _moduleRepository = moduleRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<GetModuleDto>> GetModules(CancellationToken cancellation = default)
    {
        try
        {
            return await _moduleRepository.GetModules(cancellation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener los módulos");
            return new List<GetModuleDto>();
        }
    }

    public async Task<GetModuleDto?> GetModuleById(int id, CancellationToken cancellation = default)
    {
        try
        {
            return await _moduleRepository.GetModuleById(id, cancellation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener el módulo con id {Id}", id);
            return null;
        }
    }

    public async Task<GetModuleDto> SaveOrEditModule(GetModuleDto module, CancellationToken cancellation = default)
    {
        var data = new GetModuleDto();
        try
        {
            var saveData = new Module
            {
                Id = module.Id,
                Name = module.Name.Trim(),
                IsActive = module.IsActive,
                CreatedAt = module.CreatedAt ?? DateTime.UtcNow
            };

            var isExist = await ModuleValidation(module.Name, cancellation);
            if (saveData.Id == 0 && !isExist)
            {
                data.CreatedAt = DateTime.UtcNow;
                await _moduleRepository.SaveModule(saveData, cancellation);
            }
            else
            {
                data.UpdatedAt = DateTime.UtcNow;
                await _moduleRepository.UpdateModule(saveData, cancellation);
            }

            data = await _moduleRepository.GetModuleName(saveData.Name, cancellation) ?? new();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al guardar o editar el módulo con id {Id}", module.Id);
        }
        return data;
    }

    private async Task<bool> ModuleValidation(string name, CancellationToken cancellation = default)
    {
        try
        {
            var module = await _moduleRepository.GetModuleName(name, cancellation);
            return module != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al validar el módulo con nombre {Name}", name);
            return false;
        }
    }
}
