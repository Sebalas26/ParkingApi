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

        SetControllerUser("1", "admin");
    }

    private void SetControllerUser(string? userId, string username = "admin")
    {
        var claims = new List<Claim>();
        if (!string.IsNullOrEmpty(userId))
        {
            claims.Add(new Claim(ClaimTypes.Sid, userId));
            claims.Add(new Claim(ClaimTypes.Name, username));
        }

        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
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
    public async Task Authenticate_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        _authServiceMock
            .Setup(s => s.LoginStandardAsync(It.IsAny<LoginDto>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Auth server down"));

        // Act
        var result = await _controller.Authenticate(new AuthDto(), CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task LoginMobile_WhenSuccessful_ShouldReturnOk()
    {
        // Arrange
        var request = new LoginMobileDto { Email = "cajero1@test.com", Password = "123" };
        var response = new LoginResponseDto { Token = "mobile_jwt", UserId = 1, Role = "Cajero" };
        _authServiceMock.Setup(s => s.LoginAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.LoginMobile(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(response);
    }

    [Fact]
    public async Task LoginMobile_WhenUnauthorizedAccessException_ShouldReturn401()
    {
        // Arrange
        var request = new LoginMobileDto { Email = "bad@test.com", Password = "bad" };
        _authServiceMock.Setup(s => s.LoginAsync(request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("Dispositivo no autorizado."));

        // Act
        var result = await _controller.LoginMobile(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task LoginMobile_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        _authServiceMock.Setup(s => s.LoginAsync(It.IsAny<LoginMobileDto>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Fatal error"));

        // Act
        var result = await _controller.LoginMobile(new LoginMobileDto(), CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task LoginStandard_WhenValid_ShouldReturnOk()
    {
        // Arrange
        var request = new LoginDto { Username = "admin", Password = "password" };
        var response = new AuthResponseDto { Success = true, Token = "token123" };
        _authServiceMock.Setup(s => s.LoginStandardAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.LoginStandard(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(response);
    }

    [Fact]
    public async Task LoginStandard_WhenInvalid_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new LoginDto { Username = "user", Password = "bad" };
        var response = new AuthResponseDto { Success = false, ErrorMessage = "Error" };
        _authServiceMock.Setup(s => s.LoginStandardAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.LoginStandard(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task LoginStandard_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        _authServiceMock.Setup(s => s.LoginStandardAsync(It.IsAny<LoginDto>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Internal error"));

        // Act
        var result = await _controller.LoginStandard(new LoginDto(), CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task ForgotPassword_WhenEmailEmpty_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new ForgotPasswordDto { Email = "" };

        // Act
        var result = await _controller.ForgotPassword(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task ForgotPassword_WhenSuccessful_ShouldReturnOk()
    {
        // Arrange
        var request = new ForgotPasswordDto { Email = "user@test.com" };
        _authServiceMock.Setup(s => s.GeneratePasswordResetTokenAsync("user@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.ForgotPassword(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task ForgotPassword_WhenUserNotFound_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new ForgotPasswordDto { Email = "notfound@test.com" };
        _authServiceMock.Setup(s => s.GeneratePasswordResetTokenAsync("notfound@test.com", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("Usuario no encontrado."));

        // Act
        var result = await _controller.ForgotPassword(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task ForgotPassword_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        var request = new ForgotPasswordDto { Email = "user@test.com" };
        _authServiceMock.Setup(s => s.GeneratePasswordResetTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Email service down"));

        // Act
        var result = await _controller.ForgotPassword(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task ResetPassword_WhenSuccessful_ShouldReturnOk()
    {
        // Arrange
        var request = new ResetPasswordDto { Token = "token", NewPassword = "newPassword123" };
        _authServiceMock.Setup(s => s.ResetPasswordAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.ResetPassword(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task ResetPassword_WhenInvalidToken_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new ResetPasswordDto { Token = "invalid", NewPassword = "newPassword123" };
        _authServiceMock.Setup(s => s.ResetPasswordAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.ResetPassword(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task ResetPassword_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        _authServiceMock.Setup(s => s.ResetPasswordAsync(It.IsAny<ResetPasswordDto>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Hashing failed"));

        // Act
        var result = await _controller.ResetPassword(new ResetPasswordDto(), CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task ChangePassword_WhenSidMissing_ShouldReturnUnauthorized()
    {
        // Arrange
        SetControllerUser(null);
        var request = new ChangePasswordDto { CurrentPassword = "old", NewPassword = "new" };

        // Act
        var result = await _controller.ChangePassword(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task ChangePassword_WhenSuccessful_ShouldReturnOk()
    {
        // Arrange
        SetControllerUser("1");
        var request = new ChangePasswordDto { CurrentPassword = "old", NewPassword = "new" };
        _authServiceMock.Setup(s => s.ChangePasswordAsync(1, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.ChangePassword(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task ChangePassword_WhenFailed_ShouldReturnBadRequest()
    {
        // Arrange
        SetControllerUser("1");
        var request = new ChangePasswordDto { CurrentPassword = "old", NewPassword = "new" };
        _authServiceMock.Setup(s => s.ChangePasswordAsync(1, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.ChangePassword(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task ChangePassword_WhenUnauthorizedAccessException_ShouldReturnUnauthorized()
    {
        // Arrange
        SetControllerUser("1");
        var request = new ChangePasswordDto { CurrentPassword = "wrong", NewPassword = "new" };
        _authServiceMock.Setup(s => s.ChangePasswordAsync(1, request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("Contraseña actual incorrecta."));

        // Act
        var result = await _controller.ChangePassword(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task ChangePassword_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        SetControllerUser("1");
        _authServiceMock.Setup(s => s.ChangePasswordAsync(It.IsAny<int>(), It.IsAny<ChangePasswordDto>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.ChangePassword(new ChangePasswordDto(), CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task ValidateSession_WhenValidProfile_ShouldReturnOk()
    {
        // Arrange
        SetControllerUser("1");
        var profile = new AuthResponseDto
        {
            Success = true,
            UserId = 1,
            Username = "admin",
            FullName = "Admin Principal",
            RoleName = "Administrador",
            IsAdmin = true
        };
        _authServiceMock.Setup(s => s.ValidateSessionProfileAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        // Act
        var result = await _controller.ValidateSession(CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task ValidateSession_WhenInvalidOrExpired_ShouldReturnUnauthorized()
    {
        // Arrange
        SetControllerUser("1");
        _authServiceMock.Setup(s => s.ValidateSessionProfileAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AuthResponseDto?)null);

        // Act
        var result = await _controller.ValidateSession(CancellationToken.None);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task ValidateSession_WhenSidMissing_ShouldReturnUnauthorized()
    {
        // Arrange
        SetControllerUser(null);

        // Act
        var result = await _controller.ValidateSession(CancellationToken.None);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Logout_ShouldReturnOk()
    {
        // Arrange
        SetControllerUser("1");
        _authServiceMock.Setup(s => s.LogoutAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Logout(CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _authServiceMock.Verify(s => s.LogoutAsync(1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Logout_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        SetControllerUser("1");
        _authServiceMock.Setup(s => s.LogoutAsync(1, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Logout cache error"));

        // Act
        var result = await _controller.Logout(CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }
}
