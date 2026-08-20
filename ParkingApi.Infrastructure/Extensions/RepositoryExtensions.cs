using Microsoft.Extensions.DependencyInjection;
using ParkingApi.Domain.Interfaces.Repositories.Actions;
using ParkingApi.Domain.Interfaces.Repositories.Agreements;
using ParkingApi.Domain.Interfaces.Repositories.Discounts;
using ParkingApi.Domain.Interfaces.Repositories.IdentificationTypes;
using ParkingApi.Domain.Interfaces.Repositories.Login;
using ParkingApi.Domain.Interfaces.Repositories.Modules;
using ParkingApi.Domain.Interfaces.Repositories.Operations;
using ParkingApi.Domain.Interfaces.Repositories.PasswordResetToken;
using ParkingApi.Domain.Interfaces.Repositories.PaymentMethods;
using ParkingApi.Domain.Interfaces.Repositories.RoleActions;
using ParkingApi.Domain.Interfaces.Repositories.Stores;
using ParkingApi.Domain.Interfaces.Repositories.Tickets;
using ParkingApi.Domain.Interfaces.Repositories.UserRoleModules;
using ParkingApi.Domain.Interfaces.Repositories.UserRoles;
using ParkingApi.Domain.Interfaces.Repositories.Users;
using ParkingApi.Domain.Interfaces.Repositories.VehicleRates;
using ParkingApi.Domain.Interfaces.Services;
using ParkingApi.Infrastructure.Data.Repositories.Actions;
using ParkingApi.Infrastructure.Data.Repositories.Agreements;
using ParkingApi.Infrastructure.Data.Repositories.Discounts;
using ParkingApi.Infrastructure.Data.Repositories.IdentificationTypes;
using ParkingApi.Infrastructure.Data.Repositories.Login;
using ParkingApi.Infrastructure.Data.Repositories.Modules;
using ParkingApi.Infrastructure.Data.Repositories.Operations;
using ParkingApi.Infrastructure.Data.Repositories.PasswordResetToken;
using ParkingApi.Infrastructure.Data.Repositories.PaymentMethods;
using ParkingApi.Infrastructure.Data.Repositories.RoleActions;
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
        // Current User Context
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // Seguridad y Parametrización (Migración ApiTaller)
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
        services.AddScoped<ICommercialAgreementRepository, CommercialAgreementRepository>();
        services.AddScoped<ITicketDiscountRepository, TicketDiscountRepository>();
        services.AddScoped<ParkingApi.Domain.Interfaces.Repositories.Shifts.IShiftRepository, ParkingApi.Infrastructure.Data.Repositories.Shifts.ShiftRepository>();
        services.AddScoped<ParkingApi.Domain.Interfaces.Repositories.MonthlySubscriptions.IMonthlySubscriptionRepository, ParkingApi.Infrastructure.Data.Repositories.MonthlySubscriptions.MonthlySubscriptionRepository>();

        return services;
    }
}
