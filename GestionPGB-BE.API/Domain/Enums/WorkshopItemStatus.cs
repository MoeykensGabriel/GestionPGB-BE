namespace GestionPGB_BE.API.Domain.Enums;

public enum WorkshopItemStatus
{
    Available = 0,           // Cantidad reservada en stock
    Shortage = 1,            // Stock insuficiente — falta comprar
    PurchasedInTransit = 2,  // Comprado / en camino (marcado por depósito)
    Delivered = 3,           // Entregado al mecánico
    NotFound = 4             // Código de repuesto no existe en el sistema
}
