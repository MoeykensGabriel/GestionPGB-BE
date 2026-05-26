namespace GestionPGB_BE.API.Domain.Enums;

public enum WorkshopOrderStatus
{
    PendingReview = 0,       // Recibido, siendo procesado
    AllAvailable = 1,        // Todos los ítems reservados — listo para entregar
    WithShortages = 2,       // Hay ítems faltantes
    PurchasedInTransit = 3,  // Faltantes en camino (marcado manualmente por depósito)
    Delivered = 4            // Entregado al mecánico, stock definitivamente bajado
}
