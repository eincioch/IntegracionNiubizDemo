namespace IntegracionNiubizDemo.Domain.Entities;

public enum OrderStatus { Pending = 0, Paid = 1, Rejected = 2, Error = 3 }

public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string PurchaseNumber { get; set; } = default!; // único por comercio
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "PEN";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public string? CustomerEmail { get; set; }
}