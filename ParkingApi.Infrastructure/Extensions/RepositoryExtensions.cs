using Microsoft.Extensions.DependencyInjection;
using ParkingApi.Domain.Interfaces.Repositories;
using ParkingApi.Infrastructure.Data.Repositories;

namespace ParkingApi.Infrastructure.Extensions;

public static class RepositoryExtensions
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IParkingTicketRepository, ParkingTicketRepository>();
        services.AddScoped<IVehicleRateRepository, VehicleRateRepository>();
        services.AddScoped<IStoreRepository, StoreRepository>();
        services.AddScoped<IAgreementRepository, AgreementRepository>();
        services.AddScoped<IUserSessionRepository, UserSessionRepository>();
        return services;
    }
}
