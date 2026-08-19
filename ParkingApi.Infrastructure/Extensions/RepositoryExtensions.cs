using Microsoft.Extensions.DependencyInjection;
using ParkingApi.Domain.Interfaces.Repositories.Agreements;
using ParkingApi.Domain.Interfaces.Repositories.Auth;
using ParkingApi.Domain.Interfaces.Repositories.Base;
using ParkingApi.Domain.Interfaces.Repositories.Rates;
using ParkingApi.Domain.Interfaces.Repositories.Stores;
using ParkingApi.Domain.Interfaces.Repositories.Tickets;
using ParkingApi.Domain.Interfaces.Repositories.Users;
using ParkingApi.Infrastructure.Data.Repositories.Agreements;
using ParkingApi.Infrastructure.Data.Repositories.Auth;
using ParkingApi.Infrastructure.Data.Repositories.Base;
using ParkingApi.Infrastructure.Data.Repositories.Rates;
using ParkingApi.Infrastructure.Data.Repositories.Stores;
using ParkingApi.Infrastructure.Data.Repositories.Tickets;
using ParkingApi.Infrastructure.Data.Repositories.Users;
using ParkingApi.Infrastructure.Security;

namespace ParkingApi.Infrastructure.Extensions;

public static class RepositoryExtensions
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserSessionRepository, UserSessionRepository>();
        services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
        services.AddScoped<IParkingTicketRepository, ParkingTicketRepository>();
        services.AddScoped<IVehicleRateRepository, VehicleRateRepository>();
        services.AddScoped<IStoreRepository, StoreRepository>();
        services.AddScoped<IAgreementRepository, AgreementRepository>();
        return services;
    }
}
