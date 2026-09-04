using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ParkingApi.Domain.Dtos.PaymentMethods;

namespace ParkingApi.Domain.Interfaces.Services.PaymentMethods;

public interface IPaymentMethodService
{
    Task<IEnumerable<GetPaymentMethodDto>> GetAllAsync(int? companyId = null, CancellationToken cancellation = default);
    Task<IEnumerable<GetPaymentMethodDto>> GetAllActiveAsync(int? companyId = null, CancellationToken cancellation = default);
    Task<GetPaymentMethodDto?> GetByIdAsync(int id, CancellationToken cancellation = default);
    Task<GetPaymentMethodDto> CreateOrEditPaymentMethod(GetPaymentMethodDto paymentMethod, CancellationToken cancellation = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellation = default);
}
