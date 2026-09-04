using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using ParkingApi.Core.Services.Users;
using ParkingApi.Domain.Dtos.Users;
using ParkingApi.Domain.Interfaces.Repositories.Companies;
using ParkingApi.Domain.Interfaces.Repositories.Users;
using ParkingApi.Domain.Models;
using Xunit;

namespace ParkingApi.UnitTests;

public class UserQuotaPolicyTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<ICompanyRepository> _companyRepoMock = new();
    private readonly Mock<ILogger<UserService>> _loggerMock = new();

    [Fact]
    public async Task CreateUser_WhenQuotaExceeded_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var service = new UserService(_userRepoMock.Object, _companyRepoMock.Object, _loggerMock.Object);

        var company = new Company
        {
            Id = 10,
            Name = "Parqueadero Central",
            MaxUsers = 3
        };

        _companyRepoMock.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(company);

        // Ya hay 3 usuarios activos registrados
        _userRepoMock.Setup(r => r.GetCountByCompanyIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);

        _userRepoMock.Setup(r => r.ValidateExist(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetUsersDto?)null);

        var newUserDto = new GetUsersDto
        {
            Id = 0,
            CompanyId = 10,
            Username = "operador4",
            IdentificationNumber = "12345678",
            FirstName = "Juan",
            FirstSurname = "Pérez",
            Password = "Password123*",
            Email = "juan@test.com",
            IsActive = true
        };

        // Act
        Func<Task> act = async () => await service.CreateOrEditUser(newUserDto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*límite máximo contratado de cuentas de usuario (3)*");
    }

    [Fact]
    public async Task CreateUser_WhenUnderQuota_ShouldCreateUserSuccessfully()
    {
        // Arrange
        var service = new UserService(_userRepoMock.Object, _companyRepoMock.Object, _loggerMock.Object);

        var company = new Company
        {
            Id = 10,
            Name = "Parqueadero Central",
            MaxUsers = 5
        };

        _companyRepoMock.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(company);

        // Hay 2 usuarios activos (cupo 5)
        _userRepoMock.Setup(r => r.GetCountByCompanyIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        _userRepoMock.Setup(r => r.ValidateExist(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetUsersDto?)null);

        _userRepoMock.Setup(r => r.CreateUser(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var createdDto = new GetUsersDto
        {
            Id = 3,
            CompanyId = 10,
            Username = "operador3",
            IdentificationNumber = "12345678",
            FirstName = "Carlos",
            FirstSurname = "Gómez"
        };

        // Después de crear, ValidateExist retorna el usuario creado
        _userRepoMock.SetupSequence(r => r.ValidateExist(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetUsersDto?)null)
            .ReturnsAsync(createdDto);

        var newUserDto = new GetUsersDto
        {
            Id = 0,
            CompanyId = 10,
            Username = "operador3",
            IdentificationNumber = "12345678",
            FirstName = "Carlos",
            FirstSurname = "Gómez",
            Password = "Password123*",
            Email = "carlos@test.com",
            IsActive = true
        };

        // Act
        var result = await service.CreateOrEditUser(newUserDto);

        // Assert
        result.Should().NotBeNull();
        result!.Username.Should().Be("operador3");
        _userRepoMock.Verify(r => r.CreateUser(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
