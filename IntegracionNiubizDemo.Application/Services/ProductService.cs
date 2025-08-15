using IntegracionNiubizDemo.Application.Abstractions;
using IntegracionNiubizDemo.Domain.Entities;

namespace IntegracionNiubizDemo.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _repo;
    public ProductService(IProductRepository repo) => _repo = repo;

    public async Task<IReadOnlyList<Product>> GetProductsAsync(CancellationToken ct = default)
        => await _repo.GetAllAsync(ct);

    public async Task EnsureSeedDataAsync(CancellationToken ct = default)
    {
        if (await _repo.AnyAsync(ct)) return;

        var seed = new[]
        {
            new Product { Id = Guid.NewGuid(), Name = "Laptop",  Price = 3999.99m },
            new Product { Id = Guid.NewGuid(), Name = "Mouse",   Price = 79.90m },
            new Product { Id = Guid.NewGuid(), Name = "Teclado", Price = 149.00m }
        };

        await _repo.AddRangeAsync(seed, ct);
    }
}
