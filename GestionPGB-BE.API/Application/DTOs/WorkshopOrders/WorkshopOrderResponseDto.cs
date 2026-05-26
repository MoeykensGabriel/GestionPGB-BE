using GestionPGB_BE.API.Domain.Enums;

namespace GestionPGB_BE.API.Application.DTOs.WorkshopOrders;

public record WorkshopOrderResponseDto(
    Guid Id,
    string LicensePlate,
    WorkshopOrderStatus Status,
    string? CallbackUrl,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<WorkshopOrderItemResponseDto> Items
);

public record WorkshopOrderItemResponseDto(
    Guid Id,
    string ProductCode,
    string? ProductName,
    string? ProviderName,
    int RequestedQuantity,
    int ReservedQuantity,
    int ShortageQuantity,
    WorkshopItemStatus Status,
    DateTime UpdatedAt
);
