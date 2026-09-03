using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using ParkingApi.Core.Services.Companies;
using ParkingApi.Domain.Dtos.Companies;
using ParkingApi.Domain.Dtos.Realtime;
using ParkingApi.Domain.Interfaces.Repositories.Companies;
using ParkingApi.Domain.Interfaces.Repositories.Users;
using ParkingApi.Domain.Interfaces.Services.Realtime;
using ParkingApi.Domain.Models;
using ParkingApi.Infrastructure.Data;
using Xunit;

namespace ParkingApi.UnitTests;

public class CompanyPolicyTests
{
    private readonly Mock<ICompanyRepository> _companyRepoMock = new();
    private readonly Mock<IUserSessionRepository> _sessionRepoMock = new();
    private readonly Mock<IRealtimeNotificationService> _realtimeMock = new();
    private readonly Mock<ILogger<CompanyService>> _loggerMock = new();

    private DataContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new DataContext(options);
    }

    [Fact]
    public async Task UpdateCompany_WhenDisablingMultipleSessions_ShouldRevokeAllExcessSessionsAndNotify()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new CompanyService(
            _companyRepoMock.Object,
            _sessionRepoMock.Object,
            _realtimeMock.Object,
            context,
            _loggerMock.Object);

        var existingCompany = new Company
        {
            Id = 5,
            Name = "Empresa Multi",
            Nit = "900123456",
            Email = "empresa@test.com",
            AllowMultipleSessions = true, // Estaba activa
            MaxActiveSessionsPerUser = 3
        };

        _companyRepoMock.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCompany);

        // Retornar 2 tokens revocados al desactivar
        _sessionRepoMock.Setup(r => r.RevokeAllSessionsByCompanyIdExceptLatestAsync(5, "CompanyPolicyDisabled", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string> { "token-revocado-1", "token-revocado-2" });

        var updateDto = new UpdateCompanyDto
        {
            Name = "Empresa Multi",
            Nit = "900123456",
            Email = "empresa@test.com",
            AllowMultipleSessions = false, // Se desactiva en caliente
            MaxActiveSessionsPerUser = 1
        };

        // Act
        var result = await service.UpdateCompanyAsync(5, updateDto);

        // Assert
        result.AllowMultipleSessions.Should().BeFalse();
        result.MaxActiveSessionsPerUser.Should().Be(1);

        // Debe haber invocado la revocación masiva
        _sessionRepoMock.Verify(r => r.RevokeAllSessionsByCompanyIdExceptLatestAsync(5, "CompanyPolicyDisabled", It.IsAny<CancellationToken>()), Times.Once);

        // Debe haber notificado a los 2 sockets revocados
        _realtimeMock.Verify(r => r.NotifyCustomAsync(It.Is<ConfigNotificationDto>(n => n.EventType == "UserSessionTerminated"), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }
}
