using GestionPGB_BE.API.Domain.Enums;

namespace GestionPGB_BE.API.Application.DTOs.StockMovements;

public record MovementResponseDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    int Quantity,
    MovementType Type,
    DateTime CreatedAt,
    string CreatedBy,
    int CurrentStock
);
