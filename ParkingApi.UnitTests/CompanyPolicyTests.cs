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
        _sessionRepoMock.Setup(r => r.RevokeAllSessionsByCompanyIdAsync(5, "CompanyPolicyDisabled", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<(int UserId, string Jti)> { (1, "token-revocado-1"), (2, "token-revocado-2") });

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

        // Debe haber invocado la revocación masiva total
        _sessionRepoMock.Verify(r => r.RevokeAllSessionsByCompanyIdAsync(5, "CompanyPolicyDisabled", It.IsAny<CancellationToken>()), Times.Once);

        // Debe haber notificado a los 2 sockets revocados
        _realtimeMock.Verify(r => r.NotifyCustomAsync(It.Is<ConfigNotificationDto>(n => n.EventType == "UserSessionTerminated"), It.IsAny<CancellationToken>()), Times.Exactly(2));

        // Debe haber emitido el evento en tiempo real CompanyUpdated
        _realtimeMock.Verify(r => r.NotifyCustomAsync(It.Is<ConfigNotificationDto>(n => n.EventType == "CompanyUpdated" && n.CompanyId == 5), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateCompany_WhenInvalidPhone_ShouldThrowInvalidOperationException()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new CompanyService(_companyRepoMock.Object, _sessionRepoMock.Object, _realtimeMock.Object, context, _loggerMock.Object);

        var dto = new CreateCompanyDto
        {
            Name = "Parqueadero Express",
            Nit = "900555666",
            Email = "express@test.com",
            Phone = "12345", // Menos de 10 dígitos
            AdminUsername = "admin_express",
            AdminPassword = "Password123*",
            AdminFullName = "Carlos Perez"
        };

        // Act & Assert
        var act = async () => await service.CreateCompanyAsync(dto);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*10 dígitos*");
    }

    [Fact]
    public async Task CreateCompany_WhenDuplicateNit_ShouldThrowInvalidOperationException()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        context.Companies.Add(new Company { Id = 10, Name = "Otra Empresa", Nit = "900111222", Email = "otra@test.com" });
        await context.SaveChangesAsync();

        var service = new CompanyService(_companyRepoMock.Object, _sessionRepoMock.Object, _realtimeMock.Object, context, _loggerMock.Object);

        var dto = new CreateCompanyDto
        {
            Name = "Nueva Empresa",
            Nit = "900111222", // Duplicado
            Email = "nueva@test.com",
            Phone = "3001234567",
            AdminUsername = "admin_nueva",
            AdminPassword = "Password123*",
            AdminFullName = "Admin Nueva"
        };

        // Act & Assert
        var act = async () => await service.CreateCompanyAsync(dto);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*registrada*NIT*");
    }

    [Fact]
    public async Task CreateCompany_WhenDuplicateName_ShouldThrowInvalidOperationException()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        context.Companies.Add(new Company { Id = 11, Name = "Parqueadero Central", Nit = "900333444", Email = "central@test.com" });
        await context.SaveChangesAsync();

        var service = new CompanyService(_companyRepoMock.Object, _sessionRepoMock.Object, _realtimeMock.Object, context, _loggerMock.Object);

        var dto = new CreateCompanyDto
        {
            Name = "Parqueadero Central", // Duplicado
            Nit = "900999888",
            Email = "otro@test.com",
            Phone = "3001234567",
            AdminUsername = "admin_otro",
            AdminPassword = "Password123*",
            AdminFullName = "Admin Otro"
        };

        // Act & Assert
        var act = async () => await service.CreateCompanyAsync(dto);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*nombre comercial o razón social*ya se encuentra registrado*");
    }

    [Fact]
    public async Task CreateCompany_WhenDuplicateUsername_ShouldThrowInvalidOperationException()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        context.User.Add(new User { Id = 1, Username = "admin", Email = "admin@test.com", IdentificationNumber = "123", Password = "hash", FullName = "Admin", FirstName = "Admin", FirstSurname = "Admin" });
        await context.SaveChangesAsync();

        var service = new CompanyService(_companyRepoMock.Object, _sessionRepoMock.Object, _realtimeMock.Object, context, _loggerMock.Object);

        var dto = new CreateCompanyDto
        {
            Name = "Empresa Tres",
            Nit = "900777888",
            Email = "tres@test.com",
            Phone = "3001234567",
            AdminUsername = "admin", // Ya existe
            AdminPassword = "Password123*",
            AdminFullName = "Admin Tres"
        };

        // Act & Assert
        var act = async () => await service.CreateCompanyAsync(dto);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*nombre de usuario*ya está en uso*");
    }
}
