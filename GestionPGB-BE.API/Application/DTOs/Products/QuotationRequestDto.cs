namespace GestionPGB_BE.API.Application.DTOs.Products;

public record QuotationItemDto(Guid ProductId, int Quantity);

public record QuotationRequestDto(List<QuotationItemDto> Items);
