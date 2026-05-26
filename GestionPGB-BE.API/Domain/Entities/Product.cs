namespace GestionPGB_BE.API.Domain.Entities;

public class Product
{
    public Guid Id { get; set; }
    public string Barcode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int CurrentStock { get; set; }
    public int ReservedStock { get; set; }      // reservado para pedidos del taller (no disponible para otros)
    public int MinRequiredStock { get; set; }
    public string ProviderName { get; set; } = string.Empty;

    public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
    public ICollection<WorkshopOrderItem> WorkshopOrderItems { get; set; } = new List<WorkshopOrderItem>();

    public bool IsLowStock => CurrentStock <= MinRequiredStock;
    public int AvailableStock => CurrentStock - ReservedStock;
}
