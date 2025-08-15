using IntegracionNiubizDemo.Application.Dtos;

namespace IntegracionNiubizDemo.Application.Abstractions;

public interface INiubizGateway
{
    Task<string> GetSecurityTokenAsync(CancellationToken ct = default);
    Task<string> CreateSessionAsync(string securityToken, decimal amount, string purchaseNumber, string currency, CancellationToken ct = default);
    Task<AuthorizationResult> AuthorizeAsync(string securityToken, string transactionToken, decimal amount, string currency, string purchaseNumber, CancellationToken ct = default);
}