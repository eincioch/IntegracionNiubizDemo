using Microsoft.EntityFrameworkCore;
using IntegracionNiubizDemo.Application.Abstractions;
using IntegracionNiubizDemo.Domain.Entities;
using IntegracionNiubizDemo.Persistence.Data;

namespace IntegracionNiubizDemo.Persistence.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _db;
    public OrderRepository(AppDbContext db) => _db = db;

    public Task<Order?> GetByPurchaseNumberAsync(string purchaseNumber, CancellationToken ct = default)
        => _db.Orders.FirstOrDefaultAsync(o => o.PurchaseNumber == purchaseNumber, ct);

    public async Task AddAsync(Order order, CancellationToken ct = default)
    {
        await _db.Orders.AddAsync(order, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Order order, CancellationToken ct = default)
    {
        _db.Orders.Update(order);
        await _db.SaveChangesAsync(ct);
    }
}