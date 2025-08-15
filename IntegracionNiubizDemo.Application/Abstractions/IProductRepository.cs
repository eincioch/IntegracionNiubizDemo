using IntegracionNiubizDemo.Domain.Entities;

namespace IntegracionNiubizDemo.Application.Abstractions;

public interface IProductRepository
{
    Task<List<Product>> GetAllAsync(CancellationToken ct = default);
    Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default); // <-- nuevo
    Task AddRangeAsync(IEnumerable<Product> products, CancellationToken ct = default);
    Task<bool> AnyAsync(CancellationToken ct = default);
}
