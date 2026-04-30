using GestionPGB_BE.API.Domain.Enums;

namespace GestionPGB_BE.API.Application.DTOs.StockMovements;

public record CreateMovementDto(
    Guid ProductId,
    int Quantity,
    MovementType Type
);
