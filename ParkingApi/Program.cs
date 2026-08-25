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

<<<<<<< HEAD
=======
// Asegurar que la base de datos y los datos semilla (Seed) existan automaticamente
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();
    try
    {
        await dbContext.Database.MigrateAsync();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Nota en MigrateAsync: {ex.Message}. Intentando EnsureCreated...");
        dbContext.Database.EnsureCreated();
    }
    await DatabaseSeeder.SeedAsync(dbContext);
}

>>>>>>> fe0525406570d931699ca55b7218ad3a5a0c20c9
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

app.Run();
