using GestionPGB_BE.API.Application.DTOs.WorkshopOrders;
using GestionPGB_BE.API.Application.Services;
using GestionPGB_BE.API.Infrastructure.Auth;

namespace GestionPGB_BE.API.API.Endpoints;

public static class WorkshopOrderEndpoints
{
    public static void MapWorkshopOrderEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/workshop-orders")
            .WithTags("WorkshopOrders")
            .RequireRateLimiting("global");

        // POST /api/workshop-orders
        // Recibe pedido del taller (patente + lista de códigos + cantidades).
        // Endpoint con API key — el sistema del taller se autentica con header X-Api-Key.
        group.MapPost("/", async (CreateWorkshopOrderDto dto, IWorkshopOrderService service, HttpContext ctx) =>
        {
            if (string.IsNullOrWhiteSpace(dto.LicensePlate))
                return Results.BadRequest("La patente es obligatoria.");

            if (dto.Items is null || dto.Items.Count == 0)
                return Results.BadRequest("Debe incluir al menos un ítem.");

            if (dto.Items.Any(i => i.Quantity <= 0))
                return Results.BadRequest("Las cantidades deben ser mayores a cero.");

            var order = await service.CreateOrderAsync(dto);
            return Results.Created($"/api/workshop-orders/{order.Id}", order);
        })
        .WithName("CreateWorkshopOrder")
        .WithSummary("Recibe pedido del taller: verifica stock, reserva disponible, registra faltantes.")
        .AllowAnonymous()
        .AddEndpointFilter<ApiKeyEndpointFilter>(); // valida header X-Api-Key

        // GET /api/workshop-orders — bandeja "Pedidos de Oficina" (solo Admin)
        group.MapGet("/", async (IWorkshopOrderService service) =>
            Results.Ok(await service.GetAllAsync()))
        .WithName("GetAllWorkshopOrders")
        .WithSummary("Lista todos los pedidos del taller agrupados por patente.")
        .RequireAuthorization();

        // GET /api/workshop-orders/{id}
        group.MapGet("/{id:guid}", async (Guid id, IWorkshopOrderService service) =>
        {
            var order = await service.GetByIdAsync(id);
            return order is null ? Results.NotFound() : Results.Ok(order);
        })
        .WithName("GetWorkshopOrderById")
        .RequireAuthorization();

        // PATCH /api/workshop-orders/{id}/items/{itemId}/status
        // El personal del depósito actualiza el estado de un ítem faltante.
        group.MapPatch("/{id:guid}/items/{itemId:guid}/status",
            async (Guid id, Guid itemId, UpdateWorkshopItemStatusDto dto, IWorkshopOrderService service) =>
            {
                var (order, error) = await service.UpdateItemStatusAsync(id, itemId, dto);
                if (error is not null) return Results.BadRequest(new { error });
                return order is null ? Results.NotFound() : Results.Ok(order);
            })
        .WithName("UpdateWorkshopItemStatus")
        .WithSummary("Actualiza el estado de un ítem faltante (ej: Shortage → PurchasedInTransit).")
        .RequireAuthorization();

        // POST /api/workshop-orders/{id}/recheck
        // Re-verifica disponibilidad de ítems con faltante contra el stock actual.
        group.MapPost("/{id:guid}/recheck", async (Guid id, IWorkshopOrderService service) =>
        {
            var (order, error) = await service.RecheckAvailabilityAsync(id);
            if (error is not null) return Results.BadRequest(new { error });
            return order is null ? Results.NotFound() : Results.Ok(order);
        })
        .WithName("RecheckWorkshopOrderAvailability")
        .WithSummary("Re-verifica disponibilidad de faltantes contra el stock actual. Usar luego de recibir repuestos.")
        .RequireAuthorization();

        // POST /api/workshop-orders/{id}/confirm-delivery
        // Baja definitiva del stock y notifica al taller.
        group.MapPost("/{id:guid}/confirm-delivery", async (Guid id, IWorkshopOrderService service) =>
        {
            var (order, error) = await service.ConfirmDeliveryAsync(id);
            if (error is not null) return Results.BadRequest(new { error });
            return order is null ? Results.NotFound() : Results.Ok(order);
        })
        .WithName("ConfirmWorkshopOrderDelivery")
        .WithSummary("Confirma la entrega: baja definitiva del stock reservado y notifica al sistema del taller.")
        .RequireAuthorization();
    }
}
