using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Common.Constants;
using ParkingApi.Domain.Dtos.PaymentMethods;
using ParkingApi.Domain.Interfaces.Repositories.PaymentMethods;
using ParkingApi.Domain.Interfaces.Services.PaymentMethods;
using ParkingApi.Domain.Models;

namespace ParkingApi.Core.Services.PaymentMethods;

public class PaymentMethodService : IPaymentMethodService
{
    private readonly IPaymentMethodRepository _repository;
    private readonly ILogger<PaymentMethodService> _logger;

    public PaymentMethodService(IPaymentMethodRepository repository, ILogger<PaymentMethodService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IEnumerable<GetPaymentMethodDto>> GetAllAsync(int? companyId = null, CancellationToken cancellation = default)
    {
        try
        {
            return await _repository.GetAllAsync(companyId, cancellation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, Constants.PaymentMethodError);
            return new List<GetPaymentMethodDto>();
        }
    }

    public async Task<IEnumerable<GetPaymentMethodDto>> GetAllActiveAsync(int? companyId = null, CancellationToken cancellation = default)
    {
        try
        {
            return await _repository.GetAllActiveAsync(companyId, cancellation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, Constants.PaymentMethodError);
            return new List<GetPaymentMethodDto>();
        }
    }

    public async Task<GetPaymentMethodDto?> GetByIdAsync(int id, CancellationToken cancellation = default)
    {
        try
        {
            return await _repository.GetByIdAsync(id, cancellation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Error}: Error al obtener método de pago {Id}", Constants.PaymentMethodError, id);
            return null;
        }
    }

    public async Task<GetPaymentMethodDto> CreateOrEditPaymentMethod(GetPaymentMethodDto paymentMethod, CancellationToken cancellation = default)
    {
        var result = new GetPaymentMethodDto();
        try
        {
            var data = new PaymentMethod
            {
                Id = paymentMethod.Id,
                CompanyId = paymentMethod.CompanyId,
                Name = paymentMethod.Name.Trim(),
                Icon = paymentMethod.Icon.Trim(),
                IsActive = paymentMethod.IsActive
            };

            var existing = await _repository.ValidateExist(paymentMethod.Name, paymentMethod.CompanyId, cancellation);
            if (data.Id == 0 && existing == null)
            {
                data.CreatedAt = DateTime.UtcNow;
                await _repository.CreateAsync(data, cancellation);
            }
            else
            {
                if (data.Id == 0 && existing != null)
                {
                    data.Id = existing.Id;
                }
                data.UpdatedAt = DateTime.UtcNow;
                await _repository.UpdateAsync(data, cancellation);
            }

            result = await _repository.ValidateExist(data.Name, data.CompanyId, cancellation) ?? new();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Error}: Error al crear o editar método de pago", Constants.PaymentMethodError);
        }
        return result;
    }
}
