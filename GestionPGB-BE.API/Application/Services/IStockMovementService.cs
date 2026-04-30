using GestionPGB_BE.API.Application.DTOs;
using GestionPGB_BE.API.Application.DTOs.StockMovements;
using GestionPGB_BE.API.Domain.Enums;

namespace GestionPGB_BE.API.Application.Services;

public interface IStockMovementService
{
    Task<PagedResultDto<MovementResponseDto>> GetPagedAsync(int page, int pageSize, DateTime? from, DateTime? to);
    Task<IEnumerable<MovementResponseDto>> GetByProductIdAsync(Guid productId);
    Task<MovementResponseDto?> RegisterMovementAsync(CreateMovementDto dto, string createdBy);
    Task<MovementResponseDto?> RegisterMovementByBarcodeAsync(string barcode, MovementType type, string createdBy);
}
