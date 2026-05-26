namespace GestionPGB_BE.API.Application.DTOs.WorkshopOrders;

public record CreateWorkshopOrderDto(
    string LicensePlate,
    List<WorkshopOrderItemRequestDto> Items,
    string? CallbackUrl
);

public record WorkshopOrderItemRequestDto(
    string ProductCode,
    int Quantity
);
