using IntegracionNiubizDemo.Domain.Entities;

namespace IntegracionNiubizDemo.Application.Abstractions;

public interface IProductService
{
    Task<IReadOnlyList<Product>> GetProductsAsync(CancellationToken ct = default);
    Task EnsureSeedDataAsync(CancellationToken ct = default);
}
