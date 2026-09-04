using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace ParkingApi.Hubs;

public class ParkingHub : Hub
{
    private readonly ILogger<ParkingHub> _logger;

    public ParkingHub(ILogger<ParkingHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("[SignalR] Cliente conectado: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("[SignalR] Cliente desconectado: {ConnectionId}. Error: {Message}", Context.ConnectionId, exception?.Message ?? "Ninguno");
        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinBranchGroup(int branchId)
    {
        var groupName = $"Branch_{branchId}";
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("[SignalR] Conexión {ConnectionId} se unió al grupo de sede '{GroupName}'", Context.ConnectionId, groupName);
    }

    public async Task LeaveBranchGroup(int branchId)
    {
        var groupName = $"Branch_{branchId}";
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("[SignalR] Conexión {ConnectionId} salió del grupo de sede '{GroupName}'", Context.ConnectionId, groupName);
    }

    public async Task JoinCompanyGroup(int companyId)
    {
        var groupName = $"Company_{companyId}";
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("[SignalR] Conexión {ConnectionId} se unió al grupo de empresa '{GroupName}'", Context.ConnectionId, groupName);
    }

    public async Task LeaveCompanyGroup(int companyId)
    {
        var groupName = $"Company_{companyId}";
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("[SignalR] Conexión {ConnectionId} salió del grupo de empresa '{GroupName}'", Context.ConnectionId, groupName);
    }
}

