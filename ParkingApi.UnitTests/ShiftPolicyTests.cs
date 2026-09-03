using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using ParkingApi.Core.Services.Shifts;
using ParkingApi.Domain.Common.Enums;
using ParkingApi.Domain.Dtos.Shifts;
using ParkingApi.Domain.Interfaces.Repositories.Branches;
using ParkingApi.Domain.Interfaces.Repositories.Companies;
using ParkingApi.Domain.Interfaces.Repositories.Shifts;
using ParkingApi.Domain.Interfaces.Services;
using ParkingApi.Domain.Models;
using Xunit;

namespace ParkingApi.UnitTests;

public class ShiftPolicyTests
{
    private readonly Mock<IShiftRepository> _shiftRepoMock = new();
    private readonly Mock<IBranchRepository> _branchRepoMock = new();
    private readonly Mock<ICompanyRepository> _companyRepoMock = new();
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<ILogger<ShiftService>> _loggerMock = new();

    private ShiftService CreateService()
    {
        return new ShiftService(
            _shiftRepoMock.Object,
            _branchRepoMock.Object,
            _companyRepoMock.Object,
            _currentUserMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task OpenShift_WhenMultipleShiftsAllowed_AndUnderLimit_ShouldCreateNewShift()
    {
        // Arrange
        var service = CreateService();
        var company = new Company
        {
            Id = 1,
            AllowMultipleOpenShifts = true,
            MaxOpenShiftsPerUser = 3
        };
        _companyRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(company);

        _branchRepoMock.Setup(r => r.GetUsersByBranchIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User> { new() { Id = 10, FullName = "Operador 1" } });

        // Ya tiene 1 caja abierta
        var existingShift = new WorkShift { ShiftId = Guid.NewGuid(), CompanyId = 1, BranchId = 1, UserId = 10, Status = ShiftStatus.Open, CashRegisterName = "Caja 1" };
        _shiftRepoMock.Setup(r => r.GetActiveShiftsByUserIdAsync(10, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<WorkShift> { existingShift });

        _shiftRepoMock.Setup(r => r.AddAsync(It.IsAny<WorkShift>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((WorkShift s, CancellationToken _) => s);

        var dto = new OpenShiftRequestDto
        {
            CompanyId = 1,
            BranchId = 1,
            UserId = 10,
            CashRegisterName = "Caja 2",
            BaseAmount = 50000
        };

        // Act
        var result = await service.OpenShiftAsync(10, "Operador 1", dto);

        // Assert
        result.Should().NotBeNull();
        result!.CashRegisterName.Should().Be("Caja 2");
        result.BaseAmount.Should().Be(50000);
        _shiftRepoMock.Verify(r => r.AddAsync(It.IsAny<WorkShift>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task OpenShift_WhenMultipleShiftsAllowed_AndReachesLimit_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var service = CreateService();
        var company = new Company
        {
            Id = 1,
            AllowMultipleOpenShifts = true,
            MaxOpenShiftsPerUser = 2
        };
        _companyRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(company);

        _branchRepoMock.Setup(r => r.GetUsersByBranchIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User> { new() { Id = 10, FullName = "Operador 1" } });

        // Ya tiene 2 cajas abiertas (límite alcanzado)
        var shift1 = new WorkShift { ShiftId = Guid.NewGuid(), CompanyId = 1, BranchId = 1, UserId = 10, Status = ShiftStatus.Open };
        var shift2 = new WorkShift { ShiftId = Guid.NewGuid(), CompanyId = 1, BranchId = 2, UserId = 10, Status = ShiftStatus.Open };
        _shiftRepoMock.Setup(r => r.GetActiveShiftsByUserIdAsync(10, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<WorkShift> { shift1, shift2 });

        var dto = new OpenShiftRequestDto
        {
            CompanyId = 1,
            BranchId = 1,
            UserId = 10
        };

        // Act & Assert
        var act = async () => await service.OpenShiftAsync(10, "Operador 1", dto);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*límite máximo de 2 caja(s) abierta(s)*");
    }

    [Fact]
    public async Task OpenShift_WhenSingleShiftPolicy_AndAlreadyHasShift_ShouldReturnExistingShift()
    {
        // Arrange
        var service = CreateService();
        var company = new Company
        {
            Id = 1,
            AllowMultipleOpenShifts = false,
            MaxOpenShiftsPerUser = 1
        };
        _companyRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(company);

        _branchRepoMock.Setup(r => r.GetUsersByBranchIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User> { new() { Id = 10, FullName = "Operador 1" } });

        var existingShift = new WorkShift { ShiftId = Guid.NewGuid(), CompanyId = 1, BranchId = 1, UserId = 10, Status = ShiftStatus.Open, CashRegisterName = "Caja Principal" };
        _shiftRepoMock.Setup(r => r.GetActiveShiftsByUserIdAsync(10, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<WorkShift> { existingShift });

        var dto = new OpenShiftRequestDto
        {
            CompanyId = 1,
            BranchId = 1,
            UserId = 10
        };

        // Act
        var result = await service.OpenShiftAsync(10, "Operador 1", dto);

        // Assert
        result.Should().NotBeNull();
        result!.ShiftId.Should().Be(existingShift.ShiftId);
        _shiftRepoMock.Verify(r => r.AddAsync(It.IsAny<WorkShift>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
