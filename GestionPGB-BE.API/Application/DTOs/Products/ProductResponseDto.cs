namespace GestionPGB_BE.API.Application.DTOs.Products;

public record ProductResponseDto(
    Guid Id,
    string Barcode,
    string ItemName,
    string Description,
    int CurrentStock,
    int MinRequiredStock,
    string ProviderName,
    bool IsLowStock
);
