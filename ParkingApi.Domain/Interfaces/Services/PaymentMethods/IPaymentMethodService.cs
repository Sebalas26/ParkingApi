using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ParkingApi.Domain.Dtos.PaymentMethods;

namespace ParkingApi.Domain.Interfaces.Services.PaymentMethods;

public interface IPaymentMethodService
{
    Task<IEnumerable<GetPaymentMethodDto>> GetAllAsync(CancellationToken cancellation = default);
    Task<IEnumerable<GetPaymentMethodDto>> GetAllActiveAsync(CancellationToken cancellation = default);
    Task<GetPaymentMethodDto?> GetByIdAsync(int id, CancellationToken cancellation = default);
    Task<GetPaymentMethodDto> CreateOrEditPaymentMethod(GetPaymentMethodDto paymentMethod, CancellationToken cancellation = default);
}
