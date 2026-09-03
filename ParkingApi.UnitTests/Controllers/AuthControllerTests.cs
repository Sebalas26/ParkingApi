using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ParkingApi.Controllers;
using ParkingApi.Domain.Dtos.Auth;
using ParkingApi.Domain.Interfaces.Services.Auth;
using Xunit;

namespace ParkingApi.UnitTests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly Mock<ILogger<AuthController>> _loggerMock;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        _loggerMock = new Mock<ILogger<AuthController>>();

        _controller = new AuthController(
            _authServiceMock.Object,
            _loggerMock.Object);

        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Sid, "1"),
            new Claim(ClaimTypes.Name, "admin")
        }, "TestAuth"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    [Fact]
    public async Task Authenticate_WhenCredentialsAreValid_ShouldReturnOk()
    {
        // Arrange
        var request = new AuthDto { Username = "admin", Password = "password123" };
        var expectedResponse = new AuthResponseDto
        {
            Success = true,
            Token = "jwt_token_valid",
            UserId = 1,
            Username = "admin",
            FullName = "Admin Usuario",
            RoleName = "Administrador"
        };

        _authServiceMock
            .Setup(s => s.LoginStandardAsync(It.IsAny<LoginDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.Authenticate(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task Authenticate_WhenCredentialsAreInvalid_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new AuthDto { Username = "wrong_user", Password = "wrong_password" };
        var failedResponse = new AuthResponseDto
        {
            Success = false,
            ErrorMessage = "Credenciales incorrectas o usuario inactivo."
        };

        _authServiceMock
            .Setup(s => s.LoginStandardAsync(It.IsAny<LoginDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(failedResponse);

        // Act
        var result = await _controller.Authenticate(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Logout_ShouldReturnOk()
    {
        // Act
        var result = await _controller.Logout(CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }
}
