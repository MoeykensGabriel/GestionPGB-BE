using GestionPGB_BE.API.Application.DTOs.Products;
using GestionPGB_BE.API.Application.Services;

namespace GestionPGB_BE.API.API.Endpoints;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/products").WithTags("Products").RequireAuthorization().RequireRateLimiting("global");

        group.MapGet("/", async (IProductService service, int page = 1, int pageSize = 20, string? search = null, string? provider = null) =>
            Results.Ok(await service.GetPagedAsync(page, pageSize, search, provider)))
        .WithName("GetAllProducts");

        group.MapGet("/providers", async (IProductService service) =>
            Results.Ok(await service.GetProvidersAsync()))
        .WithName("GetProviders");

        group.MapGet("/low-stock", async (IProductService service) =>
            Results.Ok(await service.GetLowStockAsync()))
        .WithName("GetLowStockProducts");

        group.MapPost("/quotation/pdf", async (QuotationRequestDto dto, IProductService productService, IPdfService pdfService) =>
        {
            if (dto.Items is null || dto.Items.Count == 0)
                return Results.BadRequest("Debe seleccionar al menos un producto.");

            var productIds = dto.Items.Select(i => i.ProductId).ToList();
            var products = await productService.GetByIdsAsync(productIds);
            if (!products.Any())
                return Results.NotFound("Ningún producto encontrado con los IDs provistos.");

            var quantityMap = dto.Items.ToDictionary(i => i.ProductId, i => i.Quantity);
            var pdfItems = products
                .Select(p => new QuotationPdfItemDto(p, quantityMap[p.Id]))
                .ToList();

            var pdf = pdfService.GenerateQuotationPdf(pdfItems);
            return Results.File(pdf, "application/pdf", $"cotizacion_{DateTime.Now:yyyyMMdd}.pdf");
        })
        .WithName("GenerateQuotationPdf")
        .RequireAuthorization(p => p.RequireRole("Admin"));

        group.MapGet("/{id:guid}", async (Guid id, IProductService service) =>
        {
            var product = await service.GetByIdAsync(id);
            return product is null ? Results.NotFound() : Results.Ok(product);
        })
        .WithName("GetProductById");

        group.MapGet("/barcode/{barcode}", async (string barcode, IProductService service) =>
        {
            var product = await service.GetByBarcodeAsync(barcode);
            return product is null ? Results.NotFound() : Results.Ok(product);
        })
        .WithName("GetProductByBarcode");

        group.MapPost("/", async (CreateProductDto dto, IProductService service) =>
        {
            var product = await service.CreateAsync(dto);
            return Results.Created($"/api/products/{product.Id}", product);
        })
        .WithName("CreateProduct")
        .RequireAuthorization(p => p.RequireRole("Admin"));

        group.MapPut("/{id:guid}", async (Guid id, UpdateProductDto dto, IProductService service) =>
        {
            var product = await service.UpdateAsync(id, dto);
            return product is null ? Results.NotFound() : Results.Ok(product);
        })
        .WithName("UpdateProduct")
        .RequireAuthorization(p => p.RequireRole("Admin"));

        group.MapDelete("/{id:guid}", async (Guid id, IProductService service) =>
        {
            var success = await service.DeleteAsync(id);
            return success ? Results.NoContent() : Results.NotFound();
        })
        .WithName("DeleteProduct")
        .RequireAuthorization(p => p.RequireRole("Admin"));

        group.MapPost("/import", async (List<CreateProductDto> dtos, IProductService service) =>
        {
            if (dtos is null || dtos.Count == 0)
                return Results.BadRequest("No se enviaron productos.");

            var result = await service.ImportAsync(dtos);
            return Results.Ok(result);
        })
        .WithName("ImportProducts")
        .RequireAuthorization(p => p.RequireRole("Admin"));
    }
}
