using Microsoft.Extensions.DependencyInjection;
using ParkingApi.Domain.Interfaces.Repositories.Actions;
using ParkingApi.Domain.Interfaces.Repositories.Agreements;
<<<<<<< HEAD
using ParkingApi.Domain.Interfaces.Repositories.Auth;
using ParkingApi.Domain.Interfaces.Repositories.Rates;
=======
using ParkingApi.Domain.Interfaces.Repositories.Discounts;
using ParkingApi.Domain.Interfaces.Repositories.IdentificationTypes;
using ParkingApi.Domain.Interfaces.Repositories.Login;
using ParkingApi.Domain.Interfaces.Repositories.Modules;
using ParkingApi.Domain.Interfaces.Repositories.Operations;
using ParkingApi.Domain.Interfaces.Repositories.PasswordResetToken;
using ParkingApi.Domain.Interfaces.Repositories.PaymentMethods;
using ParkingApi.Domain.Interfaces.Repositories.RoleActions;
>>>>>>> 90bdfc8b254eafadbadd6661c5529f3ac113a605
using ParkingApi.Domain.Interfaces.Repositories.Stores;
using ParkingApi.Domain.Interfaces.Repositories.Tickets;
using ParkingApi.Domain.Interfaces.Repositories.UserRoleModules;
using ParkingApi.Domain.Interfaces.Repositories.UserRoles;
using ParkingApi.Domain.Interfaces.Repositories.Users;
using ParkingApi.Domain.Interfaces.Repositories.VehicleRates;
using ParkingApi.Domain.Interfaces.Services;
using ParkingApi.Infrastructure.Data.Repositories.Actions;
using ParkingApi.Infrastructure.Data.Repositories.Agreements;
<<<<<<< HEAD
using ParkingApi.Infrastructure.Data.Repositories.Auth;
using ParkingApi.Infrastructure.Data.Repositories.Rates;
=======
using ParkingApi.Infrastructure.Data.Repositories.Discounts;
using ParkingApi.Infrastructure.Data.Repositories.IdentificationTypes;
using ParkingApi.Infrastructure.Data.Repositories.Login;
using ParkingApi.Infrastructure.Data.Repositories.Modules;
using ParkingApi.Infrastructure.Data.Repositories.Operations;
using ParkingApi.Infrastructure.Data.Repositories.PasswordResetToken;
using ParkingApi.Infrastructure.Data.Repositories.PaymentMethods;
using ParkingApi.Infrastructure.Data.Repositories.RoleActions;
>>>>>>> 90bdfc8b254eafadbadd6661c5529f3ac113a605
using ParkingApi.Infrastructure.Data.Repositories.Stores;
using ParkingApi.Infrastructure.Data.Repositories.Tickets;
using ParkingApi.Infrastructure.Data.Repositories.UserRoleModules;
using ParkingApi.Infrastructure.Data.Repositories.UserRoles;
using ParkingApi.Infrastructure.Data.Repositories.Users;
using ParkingApi.Infrastructure.Data.Repositories.VehicleRates;
using ParkingApi.Infrastructure.Security;

namespace ParkingApi.Infrastructure.Extensions;

public static class RepositoryExtensions
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
<<<<<<< HEAD
=======
        // Current User Context
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // Seguridad y Parametrización (Migración ApiTaller)
>>>>>>> 90bdfc8b254eafadbadd6661c5529f3ac113a605
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserRoleRepository, UserRoleRepository>();
        services.AddScoped<IModuleRepository, ModuleRepository>();
        services.AddScoped<IOperationRepository, OperationRepository>();
        services.AddScoped<IActionRepository, ActionRepository>();
        services.AddScoped<IRoleActionRepository, RoleActionRepository>();
        services.AddScoped<IUserRoleModuleRepository, UserRoleModuleRepository>();
        services.AddScoped<IIdentificationTypeRepository, IdentificationTypeRepository>();
        services.AddScoped<IPaymentMethodRepository, PaymentMethodRepository>();
        services.AddScoped<ILoginRepository, LoginRepository>();
        services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();

        // Negocio Parqueadero (Preservación 100%)
        services.AddScoped<IParkingTicketRepository, ParkingTicketRepository>();
        services.AddScoped<IVehicleRateRepository, VehicleRateRepository>();
        services.AddScoped<IStoreRepository, StoreRepository>();
<<<<<<< HEAD
        services.AddScoped<IAgreementRepository, AgreementRepository>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddHttpContextAccessor();
=======
        services.AddScoped<ICommercialAgreementRepository, CommercialAgreementRepository>();
        services.AddScoped<ITicketDiscountRepository, TicketDiscountRepository>();

>>>>>>> 90bdfc8b254eafadbadd6661c5529f3ac113a605
        return services;
    }
}
