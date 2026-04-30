using GestionPGB_BE.API.Domain.Enums;

namespace GestionPGB_BE.API.Domain.Entities;

public class StockMovement
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public MovementType Type { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;

    public Product Product { get; set; } = null!;
}
