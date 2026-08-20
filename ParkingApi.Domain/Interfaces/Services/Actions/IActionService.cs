using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ParkingApi.Domain.Dtos.Actions;

namespace ParkingApi.Domain.Interfaces.Services.Actions;

public interface IActionService
{
    Task<IEnumerable<GetActionsDto>> GetActions(CancellationToken cancellation = default);
    Task<IEnumerable<GetActionsDto>> GetActionsActive(CancellationToken cancellation = default);
    Task<GetActionsDto?> GetActionsById(int id, CancellationToken cancellation = default);
    Task<GetActionsDto> SaveOrEditActions(GetActionsDto getActions, CancellationToken cancellation = default);
}
