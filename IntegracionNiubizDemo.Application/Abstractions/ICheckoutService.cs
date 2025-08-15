using IntegracionNiubizDemo.Application.Dtos;

namespace IntegracionNiubizDemo.Application.Abstractions;

public interface ICheckoutService
{
    Task<CheckoutInitResult> InitAsync(Guid productId, string? customerEmail, CancellationToken ct = default);
    Task<ConfirmResult> ConfirmAsync(string purchaseNumber, string transactionToken, CancellationToken ct = default);
}