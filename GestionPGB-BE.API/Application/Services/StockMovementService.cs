using GestionPGB_BE.API.Application.DTOs;
using GestionPGB_BE.API.Application.DTOs.StockMovements;
using GestionPGB_BE.API.Domain.Entities;
using GestionPGB_BE.API.Domain.Enums;
using GestionPGB_BE.API.Domain.Interfaces;

namespace GestionPGB_BE.API.Application.Services;

public class StockMovementService : IStockMovementService
{
    private readonly IStockMovementRepository _repository;

    public StockMovementService(IStockMovementRepository repository) => _repository = repository;

    public async Task<PagedResultDto<MovementResponseDto>> GetPagedAsync(int page, int pageSize, DateTime? from, DateTime? to, string? createdBy = null)
    {
        var (items, total) = await _repository.GetPagedAsync(page, pageSize, from, to, createdBy);
        return new PagedResultDto<MovementResponseDto>(items.Select(ToDto), total, page, pageSize);
    }

    public async Task<IEnumerable<MovementResponseDto>> GetByProductIdAsync(Guid productId) =>
        (await _repository.GetByProductIdAsync(productId)).Select(ToDto);

    public async Task<MovementResponseDto?> RegisterMovementAsync(CreateMovementDto dto, string createdBy)
    {
        var movement = await _repository.RegisterMovementAsync(dto.ProductId, dto.Quantity, dto.Type, createdBy);
        return movement is null ? null : ToDto(movement);
    }

    public async Task<MovementResponseDto?> RegisterMovementByBarcodeAsync(string barcode, MovementType type, string createdBy)
    {
        var movement = await _repository.RegisterMovementByBarcodeAsync(barcode, type, createdBy);
        return movement is null ? null : ToDto(movement);
    }

    private static MovementResponseDto ToDto(StockMovement m) => new(
        m.Id,
        m.ProductId,
        m.Product?.ItemName ?? string.Empty,
        m.Quantity,
        m.Type,
        m.CreatedAt,
        m.CreatedBy,
        m.Product?.CurrentStock ?? 0
    );
}
