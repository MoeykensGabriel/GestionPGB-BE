using GestionPGB_BE.API.Domain.Enums;

namespace GestionPGB_BE.API.Domain.Entities;

public class WorkshopOrderItem
{
    public Guid Id { get; set; }
    public Guid WorkshopOrderId { get; set; }

    // FK al producto — null si el código no se encontró en el sistema
    public Guid? ProductId { get; set; }

    // Código de repuesto enviado por el taller (barcode)
    public string ProductCode { get; set; } = string.Empty;

    public int RequestedQuantity { get; set; }
    public int ReservedQuantity { get; set; }   // lo que se reservó del stock actual
    public int ShortageQuantity { get; set; }   // = RequestedQuantity - ReservedQuantity

    public WorkshopItemStatus Status { get; set; } = WorkshopItemStatus.Available;

    public DateTime UpdatedAt { get; set; }

    public WorkshopOrder WorkshopOrder { get; set; } = null!;
    public Product? Product { get; set; }
}
