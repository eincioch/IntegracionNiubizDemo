using IntegracionNiubizDemo.Domain.Entities;

namespace IntegracionNiubizDemo.Application.Abstractions;

public interface IPaymentRepository
{
    Task AddAsync(PaymentTransaction txn, CancellationToken ct = default);
    Task UpdateAsync(PaymentTransaction txn, CancellationToken ct = default);
    Task<PaymentTransaction?> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default);
}