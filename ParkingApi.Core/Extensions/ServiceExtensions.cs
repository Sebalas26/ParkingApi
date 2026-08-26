using Microsoft.Extensions.DependencyInjection;
using ParkingApi.Core.Services.Actions;
using ParkingApi.Core.Services.Agreements;
using ParkingApi.Core.Services.Analytics;
using ParkingApi.Core.Services.Auth;
using ParkingApi.Core.Services.IdentificationTypes;
using ParkingApi.Core.Services.Login;
using ParkingApi.Core.Services.Modules;
using ParkingApi.Core.Services.Operations;
using ParkingApi.Core.Services.PaymentMethods;
using ParkingApi.Core.Services.RoleActions;
using ParkingApi.Core.Services.Stores;
using ParkingApi.Core.Services.Sync;
using ParkingApi.Core.Services.Tickets;
using ParkingApi.Core.Services.UserRoleModules;
using ParkingApi.Core.Services.UserRoles;
using ParkingApi.Core.Services.Users;
using ParkingApi.Core.Services.VehicleRates;
using ParkingApi.Domain.Interfaces.Services.Actions;
using ParkingApi.Domain.Interfaces.Services.Agreements;
using ParkingApi.Domain.Interfaces.Services.Analytics;
using ParkingApi.Domain.Interfaces.Services.Auth;
using ParkingApi.Domain.Interfaces.Services.IdentificationTypes;
using ParkingApi.Domain.Interfaces.Services.Login;
using ParkingApi.Domain.Interfaces.Services.Modules;
using ParkingApi.Domain.Interfaces.Services.Operations;
using ParkingApi.Domain.Interfaces.Services.PaymentMethods;
using ParkingApi.Domain.Interfaces.Services.RoleActions;
using ParkingApi.Domain.Interfaces.Services.Stores;
using ParkingApi.Domain.Interfaces.Services.Sync;
using ParkingApi.Domain.Interfaces.Services.Tickets;
using ParkingApi.Domain.Interfaces.Services.UserRoleModules;
using ParkingApi.Domain.Interfaces.Services.UserRoles;
using ParkingApi.Domain.Interfaces.Services.Users;
using ParkingApi.Domain.Interfaces.Services.VehicleRates;

namespace ParkingApi.Core.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        // Seguridad y Parametrización (Migración ApiTaller)
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IUserRoleService, UserRoleService>();
        services.AddScoped<IModuleService, ModuleService>();
        services.AddScoped<IOperationService, OperationService>();
        services.AddScoped<IActionService, ActionService>();
        services.AddScoped<IRoleActionService, RoleActionService>();
        services.AddScoped<IUserRoleModuleService, UserRoleModuleService>();
        services.AddScoped<IIdentificationTypeService, IdentificationTypeService>();
        services.AddScoped<IPaymentMethodService, PaymentMethodService>();
        services.AddScoped<ILoginService, LoginService>();

        // Negocio Parqueadero (Preservación 100%)
        services.AddScoped<ParkingApi.Domain.Interfaces.Services.ParkingLots.IParkingLotService, ParkingApi.Core.Services.ParkingLots.ParkingLotService>();
        services.AddScoped<IParkingTicketService, ParkingTicketService>();
        services.AddScoped<IVehicleRateService, VehicleRateService>();
        services.AddScoped<IStoreService, StoreService>();
        services.AddScoped<ICommercialAgreementService, CommercialAgreementService>();
        services.AddScoped<IAnalyticsService, AnalyticsService>();
        services.AddScoped<ISyncService, SyncService>();
        services.AddScoped<ParkingApi.Domain.Interfaces.Services.Shifts.IShiftService, ParkingApi.Core.Services.Shifts.ShiftService>();
        services.AddScoped<ParkingApi.Domain.Interfaces.Services.MonthlySubscriptions.IMonthlySubscriptionService, ParkingApi.Core.Services.MonthlySubscriptions.MonthlySubscriptionService>();
        services.AddScoped<ParkingApi.Domain.Interfaces.Services.Branches.IBranchService, ParkingApi.Core.Services.Branches.BranchService>();
        services.AddScoped<ParkingApi.Domain.Interfaces.Services.Billing.IBillingResolutionService, ParkingApi.Core.Services.Billing.BillingResolutionService>();
        services.AddScoped<ParkingApi.Domain.Interfaces.Services.Incidents.IVehicleIncidentService, ParkingApi.Core.Services.Incidents.VehicleIncidentService>();

        return services;
    }
}
