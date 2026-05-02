using GestionPGB_BE.API.Application.DTOs;
using GestionPGB_BE.API.Application.DTOs.Products;

namespace GestionPGB_BE.API.Application.Services;

public interface IProductService
{
    Task<PagedResultDto<ProductResponseDto>> GetPagedAsync(int page, int pageSize, string? search);
    Task<ProductResponseDto?> GetByIdAsync(Guid id);
    Task<ProductResponseDto?> GetByBarcodeAsync(string barcode);
    Task<IEnumerable<ProductResponseDto>> GetLowStockAsync();
    Task<IEnumerable<ProductResponseDto>> GetByIdsAsync(IEnumerable<Guid> ids);
    Task<ProductResponseDto> CreateAsync(CreateProductDto dto);
    Task<ImportResultDto> ImportAsync(IEnumerable<CreateProductDto> dtos);
    Task<ProductResponseDto?> UpdateAsync(Guid id, UpdateProductDto dto);
    Task<bool> DeleteAsync(Guid id);
}
