using System.Security.Claims;
using GestionPGB_BE.API.API.Hubs;
using GestionPGB_BE.API.Application.DTOs.StockMovements;
using GestionPGB_BE.API.Application.Services;
using Microsoft.AspNetCore.SignalR;

namespace GestionPGB_BE.API.API.Endpoints;

public static class StockMovementEndpoints
{
    public static void MapStockMovementEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/movements").WithTags("StockMovements").RequireAuthorization().RequireRateLimiting("global");

        group.MapGet("/", async (IStockMovementService service, int page = 1, int pageSize = 25, DateTime? from = null, DateTime? to = null) =>
            Results.Ok(await service.GetPagedAsync(page, pageSize, from, to)))
        .WithName("GetAllMovements");

        group.MapGet("/product/{productId:guid}", async (Guid productId, IStockMovementService service) =>
            Results.Ok(await service.GetByProductIdAsync(productId)))
        .WithName("GetMovementsByProduct");

        group.MapPost("/", async (
            CreateMovementDto dto,
            IStockMovementService service,
            IHubContext<StockHub> hubContext,
            ClaimsPrincipal user) =>
        {
            var createdBy = user.FindFirstValue(ClaimTypes.Name) ?? "unknown";
            var movement = await service.RegisterMovementAsync(dto, createdBy);

            if (movement is null) return Results.NotFound("Producto no encontrado.");

            await hubContext.Clients.All.SendAsync("StockUpdated", movement);
            return Results.Created($"/api/movements/{movement.Id}", movement);
        })
        .WithName("RegisterMovement");

        group.MapPost("/scan", async (
            ScanBarcodeDto dto,
            IStockMovementService service,
            IHubContext<StockHub> hubContext,
            ClaimsPrincipal user) =>
        {
            var createdBy = user.FindFirstValue(ClaimTypes.Name) ?? "unknown";
            var movement = await service.RegisterMovementByBarcodeAsync(dto.Barcode, dto.Type, createdBy);

            if (movement is null)
                return Results.NotFound($"Producto con barcode '{dto.Barcode}' no encontrado.");

            await hubContext.Clients.All.SendAsync("StockUpdated", movement);
            return Results.Created($"/api/movements/{movement.Id}", movement);
        })
        .WithName("ScanBarcode");
    }
}
