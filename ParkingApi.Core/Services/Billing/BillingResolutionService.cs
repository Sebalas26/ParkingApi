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

public class BillingResolutionService : IBillingResolutionService
{
    private readonly IBillingResolutionRepository _repository;
    private readonly ILogger<BillingResolutionService> _logger;

    public BillingResolutionService(IBillingResolutionRepository repository, ILogger<BillingResolutionService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IReadOnlyList<BillingResolutionDto>> GetAllAsync(int? branchId = null, CancellationToken cancellationToken = default)
    {
        var list = await _repository.GetAllAsync(branchId, cancellationToken);
        return list.Select(MapToDto).ToList();
    }

    public async Task<IReadOnlyList<BillingResolutionDto>> GetActiveAsync(int? branchId = null, CancellationToken cancellationToken = default)
    {
        var list = await _repository.GetActiveAsync(branchId, cancellationToken);
        return list.Select(MapToDto).ToList();
    }

    public async Task<BillingResolutionDto?> GetByIdAsync(Guid resolutionId, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(resolutionId, cancellationToken);
        return entity != null ? MapToDto(entity) : null;
    }

    public async Task<BillingResolutionDto> CreateAsync(SaveBillingResolutionDto dto, CancellationToken cancellationToken = default)
    {
        var entity = new BillingResolution
        {
            ResolutionId = dto.ResolutionId ?? Guid.NewGuid(),
            CompanyId = dto.CompanyId,
            BranchId = dto.BranchId,
            Name = dto.Name.Trim(),
            DocumentType = dto.DocumentType.Trim(),
            Prefix = dto.Prefix.Trim().ToUpper(),
            ResolutionNumber = dto.ResolutionNumber.Trim(),
            FromNumber = dto.FromNumber,
            ToNumber = dto.ToNumber,
            CurrentNumber = dto.CurrentNumber > 0 ? dto.CurrentNumber : dto.FromNumber,
            ValidFrom = dto.ValidFrom,
            ValidTo = dto.ValidTo,
            TechnicalKey = dto.TechnicalKey?.Trim(),
            IsActive = dto.IsActive,
            CreatedAtUtc = DateTime.UtcNow
        };

        var created = await _repository.AddAsync(entity, cancellationToken);
        return MapToDto(created);
    }

    public async Task<BillingResolutionDto?> UpdateAsync(Guid resolutionId, SaveBillingResolutionDto dto, CancellationToken cancellationToken = default)
    {
        var entity = new BillingResolution
        {
            ResolutionId = resolutionId,
            CompanyId = dto.CompanyId,
            BranchId = dto.BranchId,
            Name = dto.Name.Trim(),
            DocumentType = dto.DocumentType.Trim(),
            Prefix = dto.Prefix.Trim().ToUpper(),
            ResolutionNumber = dto.ResolutionNumber.Trim(),
            FromNumber = dto.FromNumber,
            ToNumber = dto.ToNumber,
            CurrentNumber = dto.CurrentNumber,
            ValidFrom = dto.ValidFrom,
            ValidTo = dto.ValidTo,
            TechnicalKey = dto.TechnicalKey?.Trim(),
            IsActive = dto.IsActive
        };

        var updated = await _repository.UpdateAsync(entity, cancellationToken);
        return updated != null ? MapToDto(updated) : null;
    }

    public async Task<bool> DeactivateAsync(Guid resolutionId, CancellationToken cancellationToken = default)
    {
        return await _repository.DeactivateAsync(resolutionId, cancellationToken);
    }

    private static BillingResolutionDto MapToDto(BillingResolution r)
    {
        return new BillingResolutionDto
        {
            ResolutionId = r.ResolutionId,
            CompanyId = r.CompanyId,
            BranchId = r.BranchId,
            BranchName = r.Branch?.Name,
            Name = r.Name,
            DocumentType = r.DocumentType,
            Prefix = r.Prefix,
            ResolutionNumber = r.ResolutionNumber,
            FromNumber = r.FromNumber,
            ToNumber = r.ToNumber,
            CurrentNumber = r.CurrentNumber,
            ValidFrom = r.ValidFrom,
            ValidTo = r.ValidTo,
            TechnicalKey = r.TechnicalKey,
            IsActive = r.IsActive,
            CreatedAtUtc = r.CreatedAtUtc,
            UpdatedAtUtc = r.UpdatedAtUtc
        };
    }
}
