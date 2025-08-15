using IntegracionNiubizDemo.Domain.Entities;

namespace IntegracionNiubizDemo.Application.Abstractions;

public interface IOrderRepository
{
    Task<Order?> GetByPurchaseNumberAsync(string purchaseNumber, CancellationToken ct = default);
    Task AddAsync(Order order, CancellationToken ct = default);
    Task UpdateAsync(Order order, CancellationToken ct = default);
}