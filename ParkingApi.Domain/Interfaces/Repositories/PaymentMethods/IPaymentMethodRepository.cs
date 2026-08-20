using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ParkingApi.Domain.Dtos.PaymentMethods;
using ParkingApi.Domain.Models;

namespace ParkingApi.Domain.Interfaces.Repositories.PaymentMethods;

public interface IPaymentMethodRepository
{
    Task<IEnumerable<GetPaymentMethodDto>> GetAllAsync(CancellationToken cancellation = default);
    Task<IEnumerable<GetPaymentMethodDto>> GetAllActiveAsync(CancellationToken cancellation = default);
    Task<GetPaymentMethodDto?> GetByIdAsync(int id, CancellationToken cancellation = default);
    Task<bool> CreateAsync(PaymentMethod paymentMethod, CancellationToken cancellation = default);
    Task<bool> UpdateAsync(PaymentMethod paymentMethod, CancellationToken cancellation = default);
    Task<GetPaymentMethodDto?> ValidateExist(string name, CancellationToken cancellation = default);
}
