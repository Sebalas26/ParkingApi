using Microsoft.Extensions.DependencyInjection;
using ParkingApi.Core.Services.Analytics;
using ParkingApi.Core.Services.Auth;
using ParkingApi.Core.Services.Sync;
using ParkingApi.Core.Services.Tickets;
using ParkingApi.Core.Services.Users;
using ParkingApi.Domain.Interfaces.Services;

namespace ParkingApi.Core.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IParkingTicketService, ParkingTicketService>();
        services.AddScoped<ISyncService, SyncService>();
        services.AddScoped<IAnalyticsService, AnalyticsService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IVehicleRateService, VehicleRateService>();
        services.AddScoped<IStoreService, StoreService>();
        services.AddScoped<IAgreementService, CommercialAgreementService>();
        return services;
    }
}
