using GestionPGB_BE.API.Domain.Entities;
using GestionPGB_BE.API.Domain.Interfaces;
using GestionPGB_BE.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionPGB_BE.API.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _context;

    public ProductRepository(AppDbContext context) => _context = context;

    public async Task<(IEnumerable<Product> Items, int Total)> GetPagedAsync(int page, int pageSize, string? search)
    {
        var query = _context.Products.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(p =>
                p.Barcode.ToLower().Contains(s) ||
                p.ItemName.ToLower().Contains(s) ||
                p.ProviderName.ToLower().Contains(s));
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(p => p.ItemName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<int> GetTotalCountAsync() =>
        await _context.Products.CountAsync();

    public async Task<Product?> GetByIdAsync(Guid id) =>
        await _context.Products.FindAsync(id);

    public async Task<Product?> GetByBarcodeAsync(string barcode) =>
        await _context.Products.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Barcode == barcode);

    public async Task<IEnumerable<Product>> GetLowStockAsync() =>
        await _context.Products.AsNoTracking()
            .Where(p => p.CurrentStock <= p.MinRequiredStock)
            .OrderBy(p => p.ProviderName)
            .ThenBy(p => p.ItemName)
            .ToListAsync();

    public async Task<IEnumerable<Product>> GetByIdsAsync(IEnumerable<Guid> ids) =>
        await _context.Products.AsNoTracking()
            .Where(p => ids.Contains(p.Id))
            .OrderBy(p => p.ProviderName)
            .ThenBy(p => p.ItemName)
            .ToListAsync();

    public async Task<Product> CreateAsync(Product product)
    {
        product.Id = Guid.NewGuid();
        product.Barcode = product.Barcode.Trim();
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return product;
    }

    public async Task<(int imported, int skipped, List<string> skippedBarcodes)> ImportAsync(IEnumerable<Product> products)
    {
        var existingBarcodes = (await _context.Products
            .AsNoTracking()
            .Select(p => p.Barcode.Trim())
            .ToListAsync())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var toInsert = new List<Product>();
        var skippedBarcodes = new List<string>();

        foreach (var product in products)
        {
            var bc = product.Barcode.Trim();
            if (existingBarcodes.Contains(bc))
            {
                skippedBarcodes.Add(bc);
            }
            else
            {
                product.Id = Guid.NewGuid();
                product.Barcode = bc;
                toInsert.Add(product);
                existingBarcodes.Add(bc); // evita duplicados dentro del mismo lote
            }
        }

        if (toInsert.Count > 0)
        {
            _context.Products.AddRange(toInsert);
            await _context.SaveChangesAsync();
        }

        return (toInsert.Count, skippedBarcodes.Count, skippedBarcodes);
    }

    public async Task<Product> UpdateAsync(Product product)
    {
        _context.Products.Update(product);
        await _context.SaveChangesAsync();
        return product;
    }

    public async Task DeleteAsync(Guid id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product is not null)
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }
    }
}
