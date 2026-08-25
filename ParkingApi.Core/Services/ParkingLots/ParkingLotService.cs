using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Dtos.IdentificationTypes;
using ParkingApi.Domain.Dtos.ParkingLots;
using ParkingApi.Domain.Dtos.UserRoles;
using ParkingApi.Domain.Dtos.Users;
using ParkingApi.Domain.Interfaces.Repositories.ParkingLots;
using ParkingApi.Domain.Interfaces.Services.ParkingLots;
using ParkingApi.Domain.Models;

namespace ParkingApi.Core.Services.ParkingLots;

public class ParkingLotService : IParkingLotService
{
    private readonly IParkingLotRepository _repository;
    private readonly ILogger<ParkingLotService> _logger;

    public ParkingLotService(IParkingLotRepository repository, ILogger<ParkingLotService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ParkingLotDto>> GetParkingLotsAsync(CancellationToken cancellationToken = default)
    {
        var list = await _repository.GetAllAsync(cancellationToken);
        return list.Select(MapToDto).ToList();
    }

    public async Task<ParkingLotDto?> GetParkingLotByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        return entity != null ? MapToDto(entity) : null;
    }

    public async Task<ParkingLotDto?> SaveOrEditParkingLotAsync(SaveParkingLotDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            ParkingLot entity;
            if (dto.Id.HasValue && dto.Id.Value > 0)
            {
                entity = await _repository.GetByIdAsync(dto.Id.Value, cancellationToken)
                    ?? new ParkingLot { Id = dto.Id.Value };

                entity.Name = dto.Name;
                entity.Description = dto.Description;
                entity.ImageUrl = dto.ImageUrl;
                entity.IsMainImage = dto.IsMainImage;
                entity.IsActive = dto.IsActive;

                await _repository.UpdateAsync(entity, cancellationToken);
            }
            else
            {
                entity = new ParkingLot
                {
                    Name = dto.Name,
                    Description = dto.Description,
                    ImageUrl = dto.ImageUrl,
                    IsMainImage = dto.IsMainImage,
                    IsActive = dto.IsActive
                };
                entity = await _repository.AddAsync(entity, cancellationToken);
            }

            if (dto.IsMainImage)
            {
                await _repository.ClearMainImageFlagExceptAsync(entity.Id, cancellationToken);
            }

            await _repository.SetEnrolledUsersAsync(entity.Id, dto.EnrolledUserIds ?? new List<int>(), cancellationToken);

            var updated = await _repository.GetByIdAsync(entity.Id, cancellationToken);
            return updated != null ? MapToDto(updated) : MapToDto(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al guardar o editar parqueadero");
            return null;
        }
    }

    public async Task<bool> DeactivateParkingLotAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity == null) return false;

        entity.IsActive = false;
        return await _repository.UpdateAsync(entity, cancellationToken);
    }

    private static ParkingLotDto MapToDto(ParkingLot entity)
    {
        return new ParkingLotDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            ImageUrl = entity.ImageUrl,
            IsMainImage = entity.IsMainImage,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            EnrolledUsers = entity.UserParkings
                .Where(up => up.User != null)
                .Select(up => new GetUsersDto
                {
                    Id = up.User.Id,
                    UserRoleId = up.User.UserRoleId,
                    IdentificationTypeId = up.User.IdentificationTypeId,
                    IdentificationNumber = up.User.IdentificationNumber,
                    FirstName = up.User.FirstName,
                    MiddleName = up.User.MiddleName,
                    FirstSurname = up.User.FirstSurname,
                    SecondLastName = up.User.SecondLastName,
                    FullName = up.User.FullName,
                    Username = up.User.Username,
                    Email = up.User.Email,
                    IsActive = up.User.IsActive,
                    CreatedAt = up.User.CreatedAt,
                    UpdatedAt = up.User.UpdatedAt,
                    UserRoleDto = up.User.UserRoleIdNavigation != null ? new GetUserRoleDto
                    {
                        IdUserRol = up.User.UserRoleIdNavigation.Id,
                        RoleName = up.User.UserRoleIdNavigation.Role,
                        IsActive = up.User.UserRoleIdNavigation.IsActive
                    } : null
                }).ToList()
        };
    }
}
