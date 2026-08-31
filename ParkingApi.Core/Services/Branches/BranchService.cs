using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Dtos.Branches;
using ParkingApi.Domain.Interfaces.Repositories.Branches;
using ParkingApi.Domain.Interfaces.Repositories.Companies;
using ParkingApi.Domain.Interfaces.Services.Branches;
using ParkingApi.Domain.Models;

namespace ParkingApi.Core.Services.Branches;

public class BranchService : IBranchService
{
    private readonly IBranchRepository _branchRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly ILogger<BranchService> _logger;

    public BranchService(
        IBranchRepository branchRepository,
        ICompanyRepository companyRepository,
        ILogger<BranchService> logger)
    {
        _branchRepository = branchRepository;
        _companyRepository = companyRepository;
        _logger = logger;
    }

    public async Task<IReadOnlyList<BranchDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var branches = await _branchRepository.GetAllAsync(cancellationToken);
        return branches.Select(MapToDto).ToList();
    }

    public async Task<IReadOnlyList<BranchDto>> GetActiveAsync(int? companyId = null, CancellationToken cancellationToken = default)
    {
        var branches = await _branchRepository.GetActiveAsync(companyId, cancellationToken);
        return branches.Select(MapToDto).ToList();
    }

    public async Task<BranchDto?> GetByIdAsync(int branchId, CancellationToken cancellationToken = default)
    {
        var branch = await _branchRepository.GetByIdAsync(branchId, cancellationToken);
        return branch != null ? MapToDto(branch) : null;
    }

    public async Task<IReadOnlyList<BranchDto>> GetBranchesByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        var branches = await _branchRepository.GetBranchesByUserIdAsync(userId, cancellationToken);
        return branches.Select(MapToDto).ToList();
    }

    public async Task<IReadOnlyList<BranchDto>> GetBranchesByCompanyIdAsync(int companyId, CancellationToken cancellationToken = default)
    {
        var branches = await _branchRepository.GetBranchesByCompanyIdAsync(companyId, cancellationToken);
        return branches.Select(MapToDto).ToList();
    }

    public async Task<BranchDto> CreateAsync(CreateBranchDto dto, CancellationToken cancellationToken = default)
    {
        if (!dto.CompanyId.HasValue || dto.CompanyId.Value <= 0)
        {
            throw new InvalidOperationException("La sede debe estar asociada a una empresa válida (CompanyId requerido).");
        }

        var company = await _companyRepository.GetByIdAsync(dto.CompanyId.Value, cancellationToken);
        if (company == null)
        {
            throw new InvalidOperationException($"La empresa con ID {dto.CompanyId.Value} no existe.");
        }

        var existingBranches = await _branchRepository.GetBranchesByCompanyIdAsync(dto.CompanyId.Value, cancellationToken);
        if (company.MaxBranches > 0 && existingBranches.Count >= company.MaxBranches)
        {
            throw new InvalidOperationException($"Has alcanzado el límite máximo contratado de sedes ({company.MaxBranches}) para tu empresa. Si necesitas más sedes, comunícate con el administrador.");
        }

        // Generación dinámica consecutiva del código de sede si no se envía
        string branchCode = dto.Code?.Trim()?.ToUpperInvariant() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(branchCode))
        {
            var rawName = company.Name?.Trim() ?? "SEDE";
            var words = rawName.Split(new[] { ' ', '-', '_' }, StringSplitOptions.RemoveEmptyEntries);
            string prefix = words.Length > 0 ? Regex.Replace(words[0].ToUpperInvariant(), @"[^A-Z0-9]", "") : "SEDE";
            if (string.IsNullOrWhiteSpace(prefix) || prefix.Length < 2) prefix = "SEDE";
            if (prefix.Length > 8) prefix = prefix.Substring(0, 8);

            int nextNum = 1;
            do
            {
                branchCode = $"{prefix}-{nextNum:D2}";
                nextNum++;
            } while (existingBranches.Any(b => b.Code.Equals(branchCode, StringComparison.OrdinalIgnoreCase)));
        }
        else
        {
            if (existingBranches.Any(b => b.Code.Equals(branchCode, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException($"Ya existe una sede con el código '{branchCode}' en esta empresa.");
            }
        }

        var branch = new Branch
        {
            CompanyId = dto.CompanyId.Value,
            Code = branchCode,
            Name = dto.Name.Trim(),
            Address = dto.Address.Trim(),
            Phone = dto.Phone?.Trim(),
            City = dto.City?.Trim(),
            TotalCapacity = dto.TotalCapacity > 0 ? dto.TotalCapacity : 100,
            Notes = dto.Notes?.Trim(),
            LogoBase64 = dto.LogoBase64?.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _branchRepository.AddAsync(branch, cancellationToken);
        return MapToDto(created);
    }

    public async Task<BranchDto?> UpdateAsync(int branchId, UpdateBranchDto dto, CancellationToken cancellationToken = default)
    {
        var branch = await _branchRepository.GetByIdAsync(branchId, cancellationToken);
        if (branch == null) return null;

        if (!string.IsNullOrWhiteSpace(dto.Code))
        {
            branch.Code = dto.Code.Trim().ToUpperInvariant();
        }
        branch.Name = dto.Name.Trim();
        branch.Address = dto.Address.Trim();
        branch.Phone = dto.Phone?.Trim();
        branch.City = dto.City?.Trim();
        branch.TotalCapacity = dto.TotalCapacity > 0 ? dto.TotalCapacity : 100;
        branch.Notes = dto.Notes?.Trim();
        if (dto.LogoBase64 != null)
        {
            branch.LogoBase64 = dto.LogoBase64.Trim();
        }
        branch.IsActive = dto.IsActive;
        branch.UpdatedAt = DateTime.UtcNow;

        var updated = await _branchRepository.UpdateAsync(branch, cancellationToken);
        return MapToDto(updated);
    }

    public async Task<bool> AssignUserAsync(AssignUserBranchDto dto, CancellationToken cancellationToken = default)
    {
        return await _branchRepository.AssignUserAsync(dto.UserId, dto.BranchId, dto.IsDefault, cancellationToken);
    }

    public async Task<bool> UnassignUserAsync(int userId, int branchId, CancellationToken cancellationToken = default)
    {
        return await _branchRepository.UnassignUserAsync(userId, branchId, cancellationToken);
    }

    public async Task<IReadOnlyList<BranchPaymentMethodDto>> GetPaymentMethodsAsync(int branchId, CancellationToken cancellationToken = default)
    {
        var methods = await _branchRepository.GetPaymentMethodsByBranchIdAsync(branchId, cancellationToken);
        return methods.Select(bpm => new BranchPaymentMethodDto
        {
            Id = bpm.Id,
            BranchId = bpm.BranchId,
            PaymentMethodId = bpm.PaymentMethodId,
            PaymentMethodName = bpm.PaymentMethod.Name,
            PaymentMethodIcon = bpm.PaymentMethod.Icon,
            RequiresCashTender = bpm.RequiresCashTender,
            IsActive = bpm.IsActive
        }).ToList();
    }

    public async Task<bool> ConfigurePaymentMethodsAsync(ConfigureBranchPaymentMethodsDto dto, CancellationToken cancellationToken = default)
    {
        return await _branchRepository.SetPaymentMethodsAsync(dto.BranchId, dto.PaymentMethodIds, cancellationToken);
    }

    public async Task<IReadOnlyList<Domain.Dtos.Users.GetUsersDto>> GetUsersByBranchIdAsync(int branchId, CancellationToken cancellationToken = default)
    {
        var users = await _branchRepository.GetUsersByBranchIdAsync(branchId, cancellationToken);
        return users.Select(u => new Domain.Dtos.Users.GetUsersDto
        {
            Id = u.Id,
            UserRoleId = u.UserRoleId,
            IdentificationTypeId = u.IdentificationTypeId,
            IdentificationNumber = u.IdentificationNumber ?? string.Empty,
            FirstName = u.FirstName ?? string.Empty,
            MiddleName = u.MiddleName ?? string.Empty,
            FirstSurname = u.FirstSurname ?? string.Empty,
            SecondLastName = u.SecondLastName ?? string.Empty,
            FullName = u.FullName,
            Email = u.Email ?? string.Empty,
            Username = u.Username,
            IsActive = u.IsActive,
            CreatedAt = u.CreatedAt,
            UpdatedAt = u.UpdatedAt
        }).ToList();
    }

    private static BranchDto MapToDto(Branch b) => new()
    {
        Id = b.Id,
        CompanyId = b.CompanyId,
        CompanyName = b.Company?.Name,
        Code = b.Code,
        Name = b.Name,
        Address = b.Address,
        Phone = b.Phone,
        City = b.City,
        TotalCapacity = b.TotalCapacity,
        Notes = b.Notes,
        LogoBase64 = b.LogoBase64,
        IsActive = b.IsActive,
        CreatedAt = b.CreatedAt
    };
}
