using GestionPGB_BE.API.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GestionPGB_BE.API.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    public DbSet<WorkshopOrder> WorkshopOrders => Set<WorkshopOrder>();
    public DbSet<WorkshopOrderItem> WorkshopOrderItems => Set<WorkshopOrderItem>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Product>(entity =>
        {
            entity.ToTable("products");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Barcode).HasColumnName("barcode").HasMaxLength(255).IsRequired();
            entity.Property(e => e.ItemName).HasColumnName("item_name").HasMaxLength(255).IsRequired();
            entity.Property(e => e.Description).HasColumnName("description").HasColumnType("text");
            entity.Property(e => e.CurrentStock).HasColumnName("current_stock").IsRequired();
            entity.Property(e => e.ReservedStock).HasColumnName("reserved_stock").IsRequired().HasDefaultValue(0);
            entity.Property(e => e.MinRequiredStock).HasColumnName("min_required_stock").IsRequired();
            entity.Property(e => e.ProviderName).HasColumnName("provider_name").HasMaxLength(255).IsRequired();
            entity.HasIndex(e => e.Barcode).IsUnique();
            entity.Ignore(e => e.IsLowStock);
            entity.Ignore(e => e.AvailableStock);
        });

        builder.Entity<WorkshopOrder>(entity =>
        {
            entity.ToTable("workshop_orders");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.LicensePlate).HasColumnName("license_plate").HasMaxLength(20).IsRequired();
            entity.Property(e => e.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();
            entity.Property(e => e.CallbackUrl).HasColumnName("callback_url").HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").IsRequired();
            entity.HasIndex(e => e.LicensePlate);
        });

        builder.Entity<WorkshopOrderItem>(entity =>
        {
            entity.ToTable("workshop_order_items");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.WorkshopOrderId).HasColumnName("workshop_order_id").IsRequired();
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.ProductCode).HasColumnName("product_code").HasMaxLength(255).IsRequired();
            entity.Property(e => e.RequestedQuantity).HasColumnName("requested_quantity").IsRequired();
            entity.Property(e => e.ReservedQuantity).HasColumnName("reserved_quantity").IsRequired();
            entity.Property(e => e.ShortageQuantity).HasColumnName("shortage_quantity").IsRequired();
            entity.Property(e => e.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").IsRequired();

            entity.HasOne(e => e.WorkshopOrder)
                .WithMany(o => o.Items)
                .HasForeignKey(e => e.WorkshopOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Product)
                .WithMany(p => p.WorkshopOrderItems)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<StockMovement>(entity =>
        {
            entity.ToTable("stock_movements");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Quantity).HasColumnName("quantity").IsRequired();
            entity.Property(e => e.Type)
                .HasColumnName("type")
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(255);

            entity.HasOne(e => e.Product)
                .WithMany(p => p.StockMovements)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
