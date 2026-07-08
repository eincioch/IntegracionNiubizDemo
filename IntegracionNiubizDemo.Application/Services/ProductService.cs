using IntegracionNiubizDemo.Application.Abstractions;
using IntegracionNiubizDemo.Domain.Entities;

namespace IntegracionNiubizDemo.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _repo;
    public ProductService(IProductRepository repo) => _repo = repo;

    public async Task<IReadOnlyList<Product>> GetProductsAsync(CancellationToken ct = default)
        => await _repo.GetAllAsync(ct);

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _repo.GetByIdAsync(id, ct);

    public async Task EnsureSeedDataAsync(CancellationToken ct = default)
    {
        // Elimina todos los productos antes de seedear (solo para desarrollo)
        var all = await _repo.GetAllAsync(ct);
        foreach (var prod in all)
            await _repo.DeleteAsync(prod.Id, ct);

        var seed = new[]
        {
            new Product { Id = Guid.NewGuid(), Name = "Laptop",  Price = 3999.99m },
            new Product { Id = Guid.NewGuid(), Name = "Mouse",   Price = 79.90m },
            new Product { Id = Guid.NewGuid(), Name = "Teclado", Price = 149.00m },
            new Product { Id = Guid.NewGuid(), Name = "Lápiz", Price = 1.00m }
           };

           await _repo.AddRangeAsync(seed, ct);
       }
}

