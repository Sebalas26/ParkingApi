using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ParkingApi.Domain.Models;
using ParkingApi.Infrastructure.Data;
using ParkingApi.Infrastructure.Data.Repositories.Users;
using Xunit;

namespace ParkingApi.UnitTests;

public class UserSessionsTests
{
    private DataContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new DataContext(options);
    }

    [Fact]
    public async Task AddSession_ShouldPersistActiveSession()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repo = new UserSessionRepository(context);
        var session = new UserSession
        {
            SessionId = Guid.NewGuid(),
            UserId = 10,
            Jti = "token-jti-1",
            DeviceInfo = "Chrome en Windows",
            ExpiresAtUtc = DateTime.UtcNow.AddHours(8),
            IsRevoked = false
        };

        // Act
        await repo.AddAsync(session);
        var isActive = await repo.IsSessionActiveAsync(10, "token-jti-1");

        // Assert
        isActive.Should().BeTrue();
    }

    [Fact]
    public async Task RevokeExcessSessions_WhenMaxIs2_ShouldRevokeOldestSession()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repo = new UserSessionRepository(context);

        var session1 = new UserSession
        {
            SessionId = Guid.NewGuid(),
            UserId = 10,
            Jti = "jti-1",
            CreatedAtUtc = DateTime.UtcNow.AddMinutes(-20),
            ExpiresAtUtc = DateTime.UtcNow.AddHours(8),
            IsRevoked = false
        };
        var session2 = new UserSession
        {
            SessionId = Guid.NewGuid(),
            UserId = 10,
            Jti = "jti-2",
            CreatedAtUtc = DateTime.UtcNow.AddMinutes(-10),
            ExpiresAtUtc = DateTime.UtcNow.AddHours(8),
            IsRevoked = false
        };

        await repo.AddAsync(session1);
        await repo.AddAsync(session2);

        // Act - Con tope 2, para agregar una 3ra sesión debe quedar 1 cupo disponible (máximo 1 previo)
        var revoked = await repo.RevokeExcessSessionsAsync(10, maxAllowed: 2, "MaxSessionsExceeded");

        // Assert
        revoked.Should().ContainSingle().Which.Should().Be("jti-1");
        (await repo.IsSessionActiveAsync(10, "jti-1")).Should().BeFalse();
        (await repo.IsSessionActiveAsync(10, "jti-2")).Should().BeTrue();
    }

    [Fact]
    public async Task RevokeAllSessionsByCompanyIdExceptLatest_ShouldRevokeSecondarySessions()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repo = new UserSessionRepository(context);

        // Crear empresa y 2 usuarios
        var company = new Company { Id = 1, Name = "Parking Test" };
        var user1 = new User { Id = 1, CompanyId = 1, Username = "operador1" };
        var user2 = new User { Id = 2, CompanyId = 1, Username = "operador2" };
        context.Companies.Add(company);
        context.User.AddRange(user1, user2);
        await context.SaveChangesAsync();

        // User 1 tiene 2 sesiones
        await repo.AddAsync(new UserSession { SessionId = Guid.NewGuid(), UserId = 1, Jti = "u1-jti-1", CreatedAtUtc = DateTime.UtcNow.AddMinutes(-30), ExpiresAtUtc = DateTime.UtcNow.AddHours(2), IsRevoked = false });
        await repo.AddAsync(new UserSession { SessionId = Guid.NewGuid(), UserId = 1, Jti = "u1-jti-2", CreatedAtUtc = DateTime.UtcNow.AddMinutes(-10), ExpiresAtUtc = DateTime.UtcNow.AddHours(2), IsRevoked = false });

        // User 2 tiene 2 sesiones
        await repo.AddAsync(new UserSession { SessionId = Guid.NewGuid(), UserId = 2, Jti = "u2-jti-1", CreatedAtUtc = DateTime.UtcNow.AddMinutes(-20), ExpiresAtUtc = DateTime.UtcNow.AddHours(2), IsRevoked = false });
        await repo.AddAsync(new UserSession { SessionId = Guid.NewGuid(), UserId = 2, Jti = "u2-jti-2", CreatedAtUtc = DateTime.UtcNow.AddMinutes(-5), ExpiresAtUtc = DateTime.UtcNow.AddHours(2), IsRevoked = false });

        // Act: Empresa desactiva multi-sesión
        var revoked = await repo.RevokeAllSessionsByCompanyIdExceptLatestAsync(companyId: 1, "CompanyPolicyDisabled");

        // Assert: Las sesiones viejas deben quedar revocadas y las más recientes activas
        revoked.Should().BeEquivalentTo(new[] { "u1-jti-1", "u2-jti-1" });
        (await repo.IsSessionActiveAsync(1, "u1-jti-1")).Should().BeFalse();
        (await repo.IsSessionActiveAsync(1, "u1-jti-2")).Should().BeTrue();
        (await repo.IsSessionActiveAsync(2, "u2-jti-1")).Should().BeFalse();
        (await repo.IsSessionActiveAsync(2, "u2-jti-2")).Should().BeTrue();
    }
}
