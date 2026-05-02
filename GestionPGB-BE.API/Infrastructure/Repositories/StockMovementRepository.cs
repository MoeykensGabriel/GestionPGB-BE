using GestionPGB_BE.API.Domain.Entities;
using GestionPGB_BE.API.Domain.Enums;
using GestionPGB_BE.API.Domain.Interfaces;
using GestionPGB_BE.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionPGB_BE.API.Infrastructure.Repositories;

public class StockMovementRepository : IStockMovementRepository
{
    private readonly AppDbContext _context;

    public StockMovementRepository(AppDbContext context) => _context = context;

    public async Task<(IEnumerable<StockMovement> Items, int Total)> GetPagedAsync(int page, int pageSize, DateTime? from, DateTime? to, string? createdBy = null)
    {
        IQueryable<StockMovement> query = _context.StockMovements.AsNoTracking().Include(m => m.Product);

        if (from.HasValue)              query = query.Where(m => m.CreatedAt >= from.Value);
        if (to.HasValue)                query = query.Where(m => m.CreatedAt <= to.Value);
        if (!string.IsNullOrEmpty(createdBy)) query = query.Where(m => m.CreatedBy == createdBy);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(m => m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<IEnumerable<StockMovement>> GetByProductIdAsync(Guid productId) =>
        await _context.StockMovements
            .AsNoTracking()
            .Include(m => m.Product)
            .Where(m => m.ProductId == productId)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();

    public async Task<StockMovement?> RegisterMovementAsync(Guid productId, int quantity, MovementType type, string createdBy)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        var product = await _context.Products.FindAsync(productId);
        if (product is null) return null;

        if (product.CurrentStock + quantity < 0)
            throw new InvalidOperationException("Sin stock suficiente para registrar la salida.");

        product.CurrentStock += quantity;

        var movement = new StockMovement
        {
            Id = Guid.NewGuid(),
            ProductId = product.Id,
            Quantity = quantity,
            Type = type,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };

        _context.StockMovements.Add(movement);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        movement.Product = product;
        return movement;
    }

    public async Task<StockMovement?> RegisterMovementByBarcodeAsync(string barcode, MovementType type, string createdBy)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        var clean = barcode.Trim();
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Barcode.Trim() == clean);
        if (product is null) return null;

        if (type == MovementType.SALIDA && product.CurrentStock <= 0)
            throw new InvalidOperationException("Sin stock disponible para este producto.");

        int quantity = type == MovementType.SALIDA ? -1 : 1;
        product.CurrentStock += quantity;

        var movement = new StockMovement
        {
            Id = Guid.NewGuid(),
            ProductId = product.Id,
            Quantity = quantity,
            Type = type,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };

        _context.StockMovements.Add(movement);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        movement.Product = product;
        return movement;
    }
}
