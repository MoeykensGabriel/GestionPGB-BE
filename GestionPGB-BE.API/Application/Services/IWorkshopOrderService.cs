using GestionPGB_BE.API.Application.DTOs.WorkshopOrders;
using GestionPGB_BE.API.Domain.Enums;

namespace GestionPGB_BE.API.Application.Services;

public interface IWorkshopOrderService
{
    /// <summary>
    /// Recibe un pedido del taller: verifica stock, hace reservas lógicas y registra faltantes.
    /// </summary>
    Task<WorkshopOrderResponseDto> CreateOrderAsync(CreateWorkshopOrderDto dto);

    /// <summary>
    /// Lista todos los pedidos del taller (bandeja "Pedidos de Oficina").
    /// </summary>
    Task<IEnumerable<WorkshopOrderResponseDto>> GetAllAsync();

    /// <summary>
    /// Detalle de un pedido.
    /// </summary>
    Task<WorkshopOrderResponseDto?> GetByIdAsync(Guid id);

    /// <summary>
    /// El personal del depósito actualiza el estado de un ítem faltante (ej: Comprado/En camino).
    /// Solo permite transiciones válidas: Shortage → PurchasedInTransit.
    /// </summary>
    Task<(WorkshopOrderResponseDto? Order, string? Error)> UpdateItemStatusAsync(Guid orderId, Guid itemId, UpdateWorkshopItemStatusDto dto);

    /// <summary>
    /// Re-verifica los ítems con Shortage contra el stock actual.
    /// Si llegaron repuestos (via ENTRADA), los reserva y actualiza el estado.
    /// </summary>
    Task<(WorkshopOrderResponseDto? Order, string? Error)> RecheckAvailabilityAsync(Guid orderId);

    /// <summary>
    /// Confirma la entrega: baja definitiva del stock reservado y notifica al taller via callbackUrl.
    /// Solo disponible cuando todos los ítems están en Available.
    /// </summary>
    Task<(WorkshopOrderResponseDto? Order, string? Error)> ConfirmDeliveryAsync(Guid orderId);
}
