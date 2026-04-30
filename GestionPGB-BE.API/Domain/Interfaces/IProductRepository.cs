using GestionPGB_BE.API.Domain.Entities;

namespace GestionPGB_BE.API.Domain.Interfaces;

public interface IProductRepository
{
    Task<(IEnumerable<Product> Items, int Total)> GetPagedAsync(int page, int pageSize, string? search);
    Task<int> GetTotalCountAsync();
    Task<Product?> GetByIdAsync(Guid id);
    Task<Product?> GetByBarcodeAsync(string barcode);
    Task<IEnumerable<Product>> GetLowStockAsync();
    Task<Product> CreateAsync(Product product);
    Task<(int imported, int skipped, List<string> skippedBarcodes)> ImportAsync(IEnumerable<Product> products);
    Task<Product> UpdateAsync(Product product);
    Task DeleteAsync(Guid id);
}
