using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ParkingApi.Controllers;
using ParkingApi.Domain.Dtos.Companies;
using ParkingApi.Domain.Interfaces.Services.Companies;
using Xunit;

namespace ParkingApi.UnitTests.Controllers;

public class CompaniesControllerTests
{
    private readonly Mock<ICompanyService> _companyServiceMock;
    private readonly Mock<ILogger<CompaniesController>> _loggerMock;
    private readonly CompaniesController _controller;

    public CompaniesControllerTests()
    {
        _companyServiceMock = new Mock<ICompanyService>();
        _loggerMock = new Mock<ILogger<CompaniesController>>();

        _controller = new CompaniesController(_companyServiceMock.Object, _loggerMock.Object);

        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Sid, "1"),
            new Claim(ClaimTypes.Name, "admin"),
            new Claim(ClaimTypes.Role, "Administrador")
        }, "TestAuth"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    [Fact]
    public async Task GetAll_WhenSuccessful_ShouldReturnOkWithCompanies()
    {
        // Arrange
        var companies = new List<CompanyDto>
        {
            new() { Id = 1, Name = "Empresa Alfa", Nit = "900111222" }
        };
        _companyServiceMock.Setup(s => s.GetAllCompaniesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(companies);

        // Act
        var result = await _controller.GetAll(CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(companies);
    }

    [Fact]
    public async Task GetAll_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        _companyServiceMock.Setup(s => s.GetAllCompaniesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database offline"));

        // Act
        var result = await _controller.GetAll(CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task GetActive_WhenSuccessful_ShouldReturnOkWithActiveCompanies()
    {
        // Arrange
        var companies = new List<CompanyDto> { new() { Id = 1, Name = "Empresa Activa" } };
        _companyServiceMock.Setup(s => s.GetActiveCompaniesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(companies);

        // Act
        var result = await _controller.GetActive(CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(companies);
    }

    [Fact]
    public async Task GetActive_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        _companyServiceMock.Setup(s => s.GetActiveCompaniesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Service failure"));

        // Act
        var result = await _controller.GetActive(CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task GetById_WhenFound_ShouldReturnOkWithCompany()
    {
        // Arrange
        var company = new CompanyDto { Id = 1, Name = "Empresa Uno" };
        _companyServiceMock.Setup(s => s.GetCompanyByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(company);

        // Act
        var result = await _controller.GetById(1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(company);
    }

    [Fact]
    public async Task GetById_WhenNotFound_ShouldReturn404()
    {
        // Arrange
        _companyServiceMock.Setup(s => s.GetCompanyByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CompanyDto?)null);

        // Act
        var result = await _controller.GetById(999, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetById_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        _companyServiceMock.Setup(s => s.GetCompanyByIdAsync(1, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database fault"));

        // Act
        var result = await _controller.GetById(1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task Create_WhenNameOrNitEmpty_ShouldReturnBadRequest()
    {
        // Arrange
        var dto = new CreateCompanyDto { Name = "", Nit = "" };

        // Act
        var result = await _controller.Create(dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_WhenAdminCredentialsEmpty_ShouldReturnBadRequest()
    {
        // Arrange
        var dto = new CreateCompanyDto { Name = "Empresa Test", Nit = "12345", AdminUsername = "", AdminPassword = "" };

        // Act
        var result = await _controller.Create(dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_WhenValid_ShouldReturnCreatedAtAction()
    {
        // Arrange
        var dto = new CreateCompanyDto
        {
            Name = "Empresa Valida",
            Nit = "900987654",
            AdminUsername = "admin_empresa",
            AdminPassword = "secretPassword1"
        };
        var created = new CompanyDto { Id = 10, Name = dto.Name, Nit = dto.Nit };
        _companyServiceMock.Setup(s => s.CreateCompanyAsync(dto, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        // Act
        var result = await _controller.Create(dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>()
            .Which.Value.Should().BeEquivalentTo(created);
    }

    [Fact]
    public async Task Create_WhenInvalidOperationException_ShouldReturnBadRequest()
    {
        // Arrange
        var dto = new CreateCompanyDto
        {
            Name = "Empresa Duplicada",
            Nit = "900987654",
            AdminUsername = "admin_empresa",
            AdminPassword = "secretPassword1"
        };
        _companyServiceMock.Setup(s => s.CreateCompanyAsync(dto, 1, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("El NIT ya está registrado."));

        // Act
        var result = await _controller.Create(dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        var dto = new CreateCompanyDto
        {
            Name = "Empresa Error",
            Nit = "900987654",
            AdminUsername = "admin_empresa",
            AdminPassword = "secretPassword1"
        };
        _companyServiceMock.Setup(s => s.CreateCompanyAsync(dto, 1, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Crash during create"));

        // Act
        var result = await _controller.Create(dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task Update_WhenSuccessful_ShouldReturnOk()
    {
        // Arrange
        var dto = new UpdateCompanyDto { Name = "Empresa Editada" };
        var updated = new CompanyDto { Id = 1, Name = "Empresa Editada" };
        _companyServiceMock.Setup(s => s.UpdateCompanyAsync(1, dto, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updated);

        // Act
        var result = await _controller.Update(1, dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(updated);
    }

    [Fact]
    public async Task Update_WhenKeyNotFoundException_ShouldReturnNotFound()
    {
        // Arrange
        var dto = new UpdateCompanyDto { Name = "Inexistente" };
        _companyServiceMock.Setup(s => s.UpdateCompanyAsync(999, dto, 1, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("Empresa no encontrada"));

        // Act
        var result = await _controller.Update(999, dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Update_WhenInvalidOperationException_ShouldReturnBadRequest()
    {
        // Arrange
        var dto = new UpdateCompanyDto { Name = "Error" };
        _companyServiceMock.Setup(s => s.UpdateCompanyAsync(1, dto, 1, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Regla de negocio violada"));

        // Act
        var result = await _controller.Update(1, dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        _companyServiceMock.Setup(s => s.UpdateCompanyAsync(1, It.IsAny<UpdateCompanyDto>(), 1, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Fatal error"));

        // Act
        var result = await _controller.Update(1, new UpdateCompanyDto(), CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task ToggleStatus_WhenSuccessful_ShouldReturnOk()
    {
        // Arrange
        _companyServiceMock.Setup(s => s.ToggleCompanyStatusAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.ToggleStatus(1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task ToggleStatus_WhenNotFound_ShouldReturn404()
    {
        // Arrange
        _companyServiceMock.Setup(s => s.ToggleCompanyStatusAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.ToggleStatus(999, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task ToggleStatus_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        _companyServiceMock.Setup(s => s.ToggleCompanyStatusAsync(1, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Failed"));

        // Act
        var result = await _controller.ToggleStatus(1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task Delete_WhenSuccessful_ShouldReturnOk()
    {
        // Arrange
        _companyServiceMock.Setup(s => s.DeleteCompanyAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Delete(1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Delete_WhenNotFound_ShouldReturn404()
    {
        // Arrange
        _companyServiceMock.Setup(s => s.DeleteCompanyAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Delete(999, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Delete_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        _companyServiceMock.Setup(s => s.DeleteCompanyAsync(1, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database deletion lock"));

        // Act
        var result = await _controller.Delete(1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }
}
