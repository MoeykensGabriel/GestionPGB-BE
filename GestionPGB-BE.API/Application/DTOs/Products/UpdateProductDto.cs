namespace GestionPGB_BE.API.Application.DTOs.Products;

public record UpdateProductDto(
    string? ItemName,
    string? Description,
    int? MinRequiredStock,
    string? ProviderName
);
