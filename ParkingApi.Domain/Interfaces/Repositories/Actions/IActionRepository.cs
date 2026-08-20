using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ParkingApi.Domain.Dtos.Actions;
using ParkingApi.Domain.Models;
using ActionModel = ParkingApi.Domain.Models.Action;

namespace ParkingApi.Domain.Interfaces.Repositories.Actions;

public interface IActionRepository
{
    Task<IEnumerable<GetActionsDto>> GetActions(CancellationToken cancellation = default);
    Task<IEnumerable<GetActionsDto>> GetActionsActive(CancellationToken cancellation = default);
    Task<GetActionsDto?> GetActionsById(int id, CancellationToken cancellation = default);
    Task<bool> SaveActions(ActionModel action, CancellationToken cancellation = default);
    Task<bool> UpdateActions(ActionModel action, CancellationToken cancellation = default);
    Task<bool> GetActionByExist(string name, int idModule, int idOperation, CancellationToken cancellation = default);
    Task<GetActionsDto?> GetActionByName(string name, int idModule, int idOperation, CancellationToken cancellation = default);
}
