using Microsoft.EntityFrameworkCore;
using IntegracionNiubizDemo.Application.Abstractions;
using IntegracionNiubizDemo.Domain.Entities;
using IntegracionNiubizDemo.Persistence.Data;

namespace IntegracionNiubizDemo.Persistence.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly AppDbContext _db;
    public PaymentRepository(AppDbContext db) => _db = db;

    public async Task AddAsync(PaymentTransaction txn, CancellationToken ct = default)
    {
        await _db.PaymentTransactions.AddAsync(txn, ct);
        await _db.SaveChangesAsync(ct);
    }

    public Task<PaymentTransaction?> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default)
        => _db.PaymentTransactions.FirstOrDefaultAsync(t => t.OrderId == orderId, ct);

    public async Task UpdateAsync(PaymentTransaction txn, CancellationToken ct = default)
    {
        _db.PaymentTransactions.Update(txn);
        await _db.SaveChangesAsync(ct);
    }
}