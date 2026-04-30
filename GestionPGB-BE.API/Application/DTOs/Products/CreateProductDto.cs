namespace GestionPGB_BE.API.Application.DTOs.Products;

public record CreateProductDto(
    string Barcode,
    string ItemName,
    string Description,
    int CurrentStock,
    int MinRequiredStock,
    string ProviderName
);
