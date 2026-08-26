using System;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using ParkingApi.Core.Extensions;
using ParkingApi.Domain.Dtos.Options;
using ParkingApi.Infrastructure.Data;
using ParkingApi.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// 1. Opciones JWT
var jwtSection = builder.Configuration.GetSection("Auth");
builder.Services.Configure<JwtOptions>(jwtSection);
var jwtOptions = jwtSection.Get<JwtOptions>() ?? new JwtOptions();
var keyBytes = Encoding.UTF8.GetBytes(jwtOptions.JwtSigningKey);

// 2. Autenticación JWT Bearer
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
        ValidateIssuer = true,
        ValidIssuer = jwtOptions.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtOptions.Audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// 3. MySQL DataContext con versión explícita
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=parkflow.db";

builder.Services.AddDbContext<DataContext>(options =>
    options.UseMySql(
        connectionString,
        new MySqlServerVersion(new Version(8, 0, 36)),
        mysqlOptions => mysqlOptions.EnableRetryOnFailure(3)
    ));

// 4. Inyección de Repositorios y Servicios
builder.Services.AddRepositories();
builder.Services.AddServices();
builder.Services.AddMemoryCache();

// 5. CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        options.JsonSerializerOptions.Converters.Add(new ParkingApi.Converters.UtcDateTimeJsonConverter());
        options.JsonSerializerOptions.Converters.Add(new ParkingApi.Converters.NullableUtcDateTimeJsonConverter());
    });

// 6. Swagger / OpenAPI Robusto con resolución de conflictos de rutas
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Park Point API",
        Version = "v1",
        Description = "API Central de Control de Acceso y Gestión de Parqueaderos (Park Point)"
    });

    options.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
    options.CustomSchemaIds(type => type.FullName ?? type.Name);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Park Point API v1");
        options.RoutePrefix = "swagger";
    });
    app.MapGet("/", () => Results.Redirect("/swagger"));
}

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Inicialización automática de esquema y columnas seguras
try
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<DataContext>();
    var conn = db.Database.GetDbConnection();
    conn.Open();
    using var cmd = conn.CreateCommand();
    cmd.CommandText = @"
        SELECT COUNT(*) FROM information_schema.COLUMNS 
        WHERE TABLE_SCHEMA = DATABASE() 
          AND TABLE_NAME = 'CommercialAgreements' 
          AND COLUMN_NAME = 'ImageUrl';
    ";
    var count = Convert.ToInt32(cmd.ExecuteScalar());
    if (count == 0)
    {
        cmd.CommandText = "ALTER TABLE `CommercialAgreements` ADD COLUMN `ImageUrl` LONGTEXT NULL;";
        cmd.ExecuteNonQuery();
        Console.WriteLine("[Schema Init] Columna ImageUrl agregada exitosamente a la tabla CommercialAgreements.");
    }

    cmd.CommandText = @"
        SELECT COUNT(*) FROM information_schema.TABLES 
        WHERE TABLE_SCHEMA = DATABASE() 
          AND TABLE_NAME = 'BillingResolutions';
    ";
    var tableCount = Convert.ToInt32(cmd.ExecuteScalar());
    if (tableCount == 0)
    {
        cmd.CommandText = @"
            CREATE TABLE `BillingResolutions` (
                `ResolutionId` CHAR(36) NOT NULL,
                `BranchId` INT NULL,
                `Name` VARCHAR(150) NOT NULL,
                `DocumentType` VARCHAR(250) NOT NULL,
                `Prefix` VARCHAR(20) NOT NULL,
                `ResolutionNumber` VARCHAR(50) NOT NULL,
                `FromNumber` BIGINT NOT NULL,
                `ToNumber` BIGINT NOT NULL,
                `CurrentNumber` BIGINT NOT NULL DEFAULT 0,
                `ValidFrom` DATETIME NOT NULL,
                `ValidTo` DATETIME NOT NULL,
                `TechnicalKey` LONGTEXT NULL,
                `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
                `CreatedAtUtc` DATETIME NOT NULL,
                `UpdatedAtUtc` DATETIME NULL,
                PRIMARY KEY (`ResolutionId`),
                INDEX `IX_BillingResolutions_BranchId` (`BranchId`),
                INDEX `IX_BillingResolutions_ResolutionNumber` (`ResolutionNumber`),
                INDEX `IX_BillingResolutions_Prefix` (`Prefix`),
                INDEX `IX_BillingResolutions_IsActive` (`IsActive`)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
        ";
        cmd.ExecuteNonQuery();
        Console.WriteLine("[Schema Init] Tabla BillingResolutions creada exitosamente en MySQL.");
    }

    cmd.CommandText = @"
        SELECT COUNT(*) FROM information_schema.TABLES 
        WHERE TABLE_SCHEMA = DATABASE() 
          AND TABLE_NAME = 'VehicleIncidents';
    ";
    var incidentTableCount = Convert.ToInt32(cmd.ExecuteScalar());
    if (incidentTableCount == 0)
    {
        cmd.CommandText = @"
            CREATE TABLE `VehicleIncidents` (
                `IncidentId` CHAR(36) NOT NULL,
                `PlateNumber` VARCHAR(20) NOT NULL,
                `BranchId` INT NULL,
                `IncidentType` VARCHAR(100) NOT NULL,
                `IsBlocked` TINYINT(1) NOT NULL DEFAULT 0,
                `Description` LONGTEXT NOT NULL,
                `ReportedBy` VARCHAR(100) NOT NULL,
                `ContactPhone` VARCHAR(30) NULL,
                `Status` VARCHAR(30) NOT NULL DEFAULT 'Activa',
                `ResolvedNotes` LONGTEXT NULL,
                `ResolvedAtUtc` DATETIME NULL,
                `CreatedAtUtc` DATETIME NOT NULL,
                `UpdatedAtUtc` DATETIME NULL,
                PRIMARY KEY (`IncidentId`),
                INDEX `IX_VehicleIncidents_PlateNumber` (`PlateNumber`),
                INDEX `IX_VehicleIncidents_BranchId` (`BranchId`),
                INDEX `IX_VehicleIncidents_IsBlocked` (`IsBlocked`),
                INDEX `IX_VehicleIncidents_Status` (`Status`)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
        ";
        cmd.ExecuteNonQuery();
        Console.WriteLine("[Schema Init] Tabla VehicleIncidents creada exitosamente en MySQL.");
    }

    cmd.CommandText = @"
        SELECT COUNT(*) FROM information_schema.COLUMNS 
        WHERE TABLE_SCHEMA = DATABASE() 
          AND TABLE_NAME = 'ParkingTickets' 
          AND COLUMN_NAME = 'ResolutionId';
    ";
    var resColCount = Convert.ToInt32(cmd.ExecuteScalar());
    if (resColCount == 0)
    {
        cmd.CommandText = @"
            ALTER TABLE `ParkingTickets` 
            ADD COLUMN `ResolutionId` CHAR(36) NULL,
            ADD COLUMN `ResolutionName` VARCHAR(150) NULL,
            ADD COLUMN `InvoiceNumber` VARCHAR(50) NULL,
            ADD COLUMN `IsElectronicInvoice` TINYINT(1) NOT NULL DEFAULT 0;
        ";
        cmd.ExecuteNonQuery();
        Console.WriteLine("[Schema Init] Columnas de resolución agregadas a ParkingTickets en MySQL.");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"[Schema Init] Advertencia durante verificación de esquema: {ex.Message}");
}

app.Run();
