using Microsoft.Extensions.DependencyInjection;
using ParkingApi.Core.Services.Agreements;
using ParkingApi.Core.Services.Analytics;
using ParkingApi.Core.Services.Auth;
using ParkingApi.Core.Services.Rates;
using ParkingApi.Core.Services.Stores;
using ParkingApi.Core.Services.Sync;
using ParkingApi.Core.Services.Tickets;
using ParkingApi.Core.Services.Users;
using ParkingApi.Domain.Interfaces.Services.Agreements;
using ParkingApi.Domain.Interfaces.Services.Analytics;
using ParkingApi.Domain.Interfaces.Services.Auth;
using ParkingApi.Domain.Interfaces.Services.Rates;
using ParkingApi.Domain.Interfaces.Services.Stores;
using ParkingApi.Domain.Interfaces.Services.Sync;
using ParkingApi.Domain.Interfaces.Services.Tickets;
using ParkingApi.Domain.Interfaces.Services.Users;

namespace ParkingApi.Core.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IParkingTicketService, ParkingTicketService>();
        services.AddScoped<IVehicleRateService, VehicleRateService>();
        services.AddScoped<IStoreService, StoreService>();
        services.AddScoped<IAgreementService, CommercialAgreementService>();
        services.AddScoped<IAnalyticsService, AnalyticsService>();
        services.AddScoped<ISyncService, SyncService>();
        return services;
    }
}
