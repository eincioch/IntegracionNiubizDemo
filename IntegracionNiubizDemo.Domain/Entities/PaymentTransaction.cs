namespace IntegracionNiubizDemo.Domain.Entities;

public class PaymentTransaction
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrderId { get; set; }
    public string? SessionKey { get; set; }
    public string? TransactionToken { get; set; }
    public string? AuthorizationCode { get; set; }
    public string? MaskedCard { get; set; }
    public string Status { get; set; } = "INIT"; // AUTHORIZED/FAILED/etc
    public string? RawResponse { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}