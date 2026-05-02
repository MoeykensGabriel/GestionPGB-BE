using GestionPGB_BE.API.Domain.Entities;
using GestionPGB_BE.API.Domain.Enums;

namespace GestionPGB_BE.API.Domain.Interfaces;

public interface IStockMovementRepository
{
    Task<(IEnumerable<StockMovement> Items, int Total)> GetPagedAsync(int page, int pageSize, DateTime? from, DateTime? to, string? createdBy = null);
    Task<IEnumerable<StockMovement>> GetByProductIdAsync(Guid productId);
    Task<StockMovement?> RegisterMovementAsync(Guid productId, int quantity, MovementType type, string createdBy);
    Task<StockMovement?> RegisterMovementByBarcodeAsync(string barcode, MovementType type, string createdBy);
}
