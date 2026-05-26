using GestionPGB_BE.API.Domain.Enums;

namespace GestionPGB_BE.API.Domain.Entities;

public class WorkshopOrder
{
    public Guid Id { get; set; }
    public string LicensePlate { get; set; } = string.Empty;
    public WorkshopOrderStatus Status { get; set; } = WorkshopOrderStatus.PendingReview;

    // URL del sistema de taller para notificar cuando el pedido sea entregado
    public string? CallbackUrl { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<WorkshopOrderItem> Items { get; set; } = new List<WorkshopOrderItem>();
}
