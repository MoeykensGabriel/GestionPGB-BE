using System.Text;
using System.Text.Json;
using GestionPGB_BE.API.Application.DTOs.WorkshopOrders;
using Microsoft.Extensions.Configuration;
using GestionPGB_BE.API.Domain.Entities;
using GestionPGB_BE.API.Domain.Enums;
using GestionPGB_BE.API.Domain.Interfaces;
using GestionPGB_BE.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionPGB_BE.API.Application.Services;

public class WorkshopOrderService : IWorkshopOrderService
{
    private readonly IWorkshopOrderRepository _repository;
    private readonly AppDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<WorkshopOrderService> _logger;
    private readonly IConfiguration _config;

    public WorkshopOrderService(
        IWorkshopOrderRepository repository,
        AppDbContext context,
        IHttpClientFactory httpClientFactory,
        ILogger<WorkshopOrderService> logger,
        IConfiguration config)
    {
        _repository = repository;
        _context = context;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _config = config;
    }

    public async Task<WorkshopOrderResponseDto> CreateOrderAsync(CreateWorkshopOrderDto dto)
    {
        var now = DateTime.UtcNow;
        var order = new WorkshopOrder
        {
            LicensePlate = dto.LicensePlate.Trim().ToUpper(),
            CallbackUrl = dto.CallbackUrl,
            CreatedAt = now,
            UpdatedAt = now
        };

        foreach (var itemDto in dto.Items)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Barcode == itemDto.ProductCode.Trim());

            var item = new WorkshopOrderItem
            {
                ProductCode = itemDto.ProductCode.Trim(),
                RequestedQuantity = itemDto.Quantity,
                UpdatedAt = now
            };

            if (product is null)
            {
                item.ProductId = null;
                item.ReservedQuantity = 0;
                item.ShortageQuantity = itemDto.Quantity;
                item.Status = WorkshopItemStatus.NotFound;
            }
            else
            {
                item.ProductId = product.Id;
                int available = product.CurrentStock - product.ReservedStock;
                int toReserve = Math.Min(available, itemDto.Quantity);
                int shortage = itemDto.Quantity - toReserve;

                item.ReservedQuantity = toReserve;
                item.ShortageQuantity = shortage;
                item.Status = shortage > 0 ? WorkshopItemStatus.Shortage : WorkshopItemStatus.Available;

                // Reserva lógica inmediata
                product.ReservedStock += toReserve;
                _context.Products.Update(product);
            }

            order.Items.Add(item);
        }

        order.Status = ResolveOrderStatus(order.Items);

        await _repository.CreateAsync(order);
        return ToDto(order);
    }

    public async Task<IEnumerable<WorkshopOrderResponseDto>> GetAllAsync() =>
        (await _repository.GetAllAsync()).Select(ToDto);

    public async Task<WorkshopOrderResponseDto?> GetByIdAsync(Guid id)
    {
        var order = await _repository.GetByIdAsync(id);
        return order is null ? null : ToDto(order);
    }

    public async Task<(WorkshopOrderResponseDto? Order, string? Error)> UpdateItemStatusAsync(
        Guid orderId, Guid itemId, UpdateWorkshopItemStatusDto dto)
    {
        var order = await _repository.GetByIdAsync(orderId);
        if (order is null) return (null, "Pedido no encontrado.");

        if (order.Status == WorkshopOrderStatus.Delivered)
            return (null, "El pedido ya fue entregado. No se puede modificar.");

        var item = order.Items.FirstOrDefault(i => i.Id == itemId);
        if (item is null) return (null, "Ítem no encontrado.");

        // Solo se puede cambiar un Shortage a PurchasedInTransit
        if (dto.Status == WorkshopItemStatus.PurchasedInTransit && item.Status != WorkshopItemStatus.Shortage)
            return (null, "Solo se puede marcar como 'Comprado/En camino' un ítem que esté en faltante.");

        item.Status = dto.Status;
        item.UpdatedAt = DateTime.UtcNow;
        order.Status = ResolveOrderStatus(order.Items);
        order.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(order);
        return (ToDto(order), null);
    }

    public async Task<(WorkshopOrderResponseDto? Order, string? Error)> RecheckAvailabilityAsync(Guid orderId)
    {
        var order = await _repository.GetByIdAsync(orderId);
        if (order is null) return (null, "Pedido no encontrado.");

        if (order.Status == WorkshopOrderStatus.Delivered)
            return (null, "El pedido ya fue entregado.");

        var now = DateTime.UtcNow;
        var shortageItems = order.Items
            .Where(i => i.Status == WorkshopItemStatus.Shortage || i.Status == WorkshopItemStatus.PurchasedInTransit)
            .ToList();

        foreach (var item in shortageItems)
        {
            if (item.ProductId is null) continue;

            var product = await _context.Products.FindAsync(item.ProductId.Value);
            if (product is null) continue;

            int available = product.CurrentStock - product.ReservedStock;
            if (available <= 0) continue;

            int toReserve = Math.Min(available, item.ShortageQuantity);
            product.ReservedStock += toReserve;
            item.ReservedQuantity += toReserve;
            item.ShortageQuantity -= toReserve;
            item.Status = item.ShortageQuantity == 0 ? WorkshopItemStatus.Available : WorkshopItemStatus.Shortage;
            item.UpdatedAt = now;

            _context.Products.Update(product);
        }

        order.Status = ResolveOrderStatus(order.Items);
        order.UpdatedAt = now;

        await _repository.UpdateAsync(order);
        return (ToDto(order), null);
    }

    public async Task<(WorkshopOrderResponseDto? Order, string? Error)> ConfirmDeliveryAsync(Guid orderId)
    {
        var order = await _repository.GetByIdAsync(orderId);
        if (order is null) return (null, "Pedido no encontrado.");

        if (order.Status == WorkshopOrderStatus.Delivered)
            return (null, "El pedido ya fue entregado.");

        bool hasNonAvailable = order.Items.Any(i =>
            i.Status != WorkshopItemStatus.Available &&
            i.Status != WorkshopItemStatus.Delivered);

        if (hasNonAvailable)
            return (null, "No se puede confirmar la entrega: hay ítems con faltantes pendientes. Use 'Verificar disponibilidad' luego de que ingresen los repuestos.");

        var now = DateTime.UtcNow;

        foreach (var item in order.Items.Where(i => i.Status == WorkshopItemStatus.Available))
        {
            if (item.ProductId is null) continue;

            var product = await _context.Products.FindAsync(item.ProductId.Value);
            if (product is null) continue;

            // Baja definitiva: descuenta del stock real y libera la reserva
            product.CurrentStock -= item.ReservedQuantity;
            product.ReservedStock -= item.ReservedQuantity;
            if (product.ReservedStock < 0) product.ReservedStock = 0;
            if (product.CurrentStock < 0) product.CurrentStock = 0;

            item.Status = WorkshopItemStatus.Delivered;
            item.UpdatedAt = now;

            _context.Products.Update(product);
        }

        order.Status = WorkshopOrderStatus.Delivered;
        order.UpdatedAt = now;

        await _repository.UpdateAsync(order);

        // Notificar al taller via callback (fire-and-forget)
        if (!string.IsNullOrEmpty(order.CallbackUrl))
            _ = SendCallbackAsync(order);

        return (ToDto(order), null);
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    private static WorkshopOrderStatus ResolveOrderStatus(ICollection<WorkshopOrderItem> items)
    {
        if (items.All(i => i.Status == WorkshopItemStatus.Available || i.Status == WorkshopItemStatus.Delivered))
            return WorkshopOrderStatus.AllAvailable;

        if (items.Any(i => i.Status == WorkshopItemStatus.PurchasedInTransit) &&
            !items.Any(i => i.Status == WorkshopItemStatus.Shortage || i.Status == WorkshopItemStatus.NotFound))
            return WorkshopOrderStatus.PurchasedInTransit;

        return WorkshopOrderStatus.WithShortages;
    }

    private async Task SendCallbackAsync(WorkshopOrder order)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("WorkshopCallback");
            var payload = new
            {
                licensePlate = order.LicensePlate,
                workshopOrderId = order.Id,
                status = order.Status.ToString(),
                deliveredAt = DateTime.UtcNow,
                items = order.Items.Select(i => new
                {
                    productCode = i.ProductCode,
                    requestedQuantity = i.RequestedQuantity,
                    deliveredQuantity = i.ReservedQuantity,
                    status = i.Status.ToString()
                })
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Autenticación saliente: GestionPGB se identifica ante MyCarApp
            var callbackApiKey =
                Environment.GetEnvironmentVariable("CALLBACK_API_KEY")
                ?? _config["CALLBACK_API_KEY"];
            if (!string.IsNullOrEmpty(callbackApiKey))
                client.DefaultRequestHeaders.Add("X-Api-Key", callbackApiKey);

            var response = await client.PostAsync(order.CallbackUrl, content);

            _logger.LogInformation(
                "Callback enviado a {CallbackUrl} para patente {LicensePlate}. StatusCode: {StatusCode}",
                order.CallbackUrl, order.LicensePlate, (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error al enviar callback para pedido {OrderId} (patente {LicensePlate})",
                order.Id, order.LicensePlate);
        }
    }

    private static WorkshopOrderResponseDto ToDto(WorkshopOrder o) => new(
        o.Id,
        o.LicensePlate,
        o.Status,
        o.CallbackUrl,
        o.CreatedAt,
        o.UpdatedAt,
        o.Items.Select(i => new WorkshopOrderItemResponseDto(
            i.Id,
            i.ProductCode,
            i.Product?.ItemName,
            i.Product?.ProviderName,
            i.RequestedQuantity,
            i.ReservedQuantity,
            i.ShortageQuantity,
            i.Status,
            i.UpdatedAt
        )).ToList()
    );
}
