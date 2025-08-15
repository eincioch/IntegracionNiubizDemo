using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IntegracionNiubizDemo.Domain.Entities;

namespace IntegracionNiubizDemo.Persistence.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(b =>
        {
            b.HasKey(p => p.Id);
            b.Property(p => p.Name).IsRequired().HasMaxLength(200);
            b.Property(p => p.Price).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Order>(b =>
        {
            b.HasKey(o => o.Id);
            b.Property(o => o.PurchaseNumber).IsRequired().HasMaxLength(20);
            b.HasIndex(o => o.PurchaseNumber).IsUnique();
            b.Property(o => o.Amount).HasPrecision(18, 2);
            b.Property(o => o.Currency).IsRequired().HasMaxLength(3);
            b.Property(o => o.CreatedAt);
            b.Property(o => o.Status).HasConversion<int>();
            b.Property(o => o.CustomerEmail).HasMaxLength(256);
        });

        modelBuilder.Entity<PaymentTransaction>(ConfigurePaymentTransaction);
    }

    private static void ConfigurePaymentTransaction(EntityTypeBuilder<PaymentTransaction> b)
    {
        b.HasKey(t => t.Id);
        b.Property(t => t.SessionKey).HasMaxLength(256);
        b.Property(t => t.TransactionToken).HasMaxLength(256);
        b.Property(t => t.AuthorizationCode).HasMaxLength(64);
        b.Property(t => t.MaskedCard).HasMaxLength(32);
        b.Property(t => t.Status).HasMaxLength(32);
        b.Property(t => t.CreatedAt);

        b.HasIndex(t => t.OrderId);
        b.HasOne<Order>()
         .WithMany()
         .HasForeignKey(t => t.OrderId)
         .OnDelete(DeleteBehavior.Cascade);
    }
}
