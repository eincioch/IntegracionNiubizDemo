using Microsoft.EntityFrameworkCore;
using IntegracionNiubizDemo.Application.Abstractions;
using IntegracionNiubizDemo.Domain.Entities;
using IntegracionNiubizDemo.Persistence.Data;

namespace IntegracionNiubizDemo.Persistence.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _db;
    public ProductRepository(AppDbContext db) => _db = db;

    public Task<bool> AnyAsync(CancellationToken ct = default)
        => _db.Products.AnyAsync(ct);

    public Task<List<Product>> GetAllAsync(CancellationToken ct = default)
        => _db.Products.AsNoTracking().ToListAsync(ct);

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task AddRangeAsync(IEnumerable<Product> products, CancellationToken ct = default)
    {
        await _db.Products.AddRangeAsync(products, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var prod = await _db.Products.FindAsync(new object[] { id }, ct);
        if (prod != null)
        {
            _db.Products.Remove(prod);
            await _db.SaveChangesAsync(ct);
        }
    }
}
