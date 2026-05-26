using GestionPGB_BE.API.Application.DTOs;
using GestionPGB_BE.API.Application.DTOs.Products;
using GestionPGB_BE.API.Domain.Entities;
using GestionPGB_BE.API.Domain.Interfaces;

namespace GestionPGB_BE.API.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _repository;

    public ProductService(IProductRepository repository) => _repository = repository;

    public async Task<PagedResultDto<ProductResponseDto>> GetPagedAsync(int page, int pageSize, string? search)
    {
        var (items, total) = await _repository.GetPagedAsync(page, pageSize, search);
        return new PagedResultDto<ProductResponseDto>(items.Select(ToDto), total, page, pageSize);
    }

    public async Task<ProductResponseDto?> GetByIdAsync(Guid id)
    {
        var product = await _repository.GetByIdAsync(id);
        return product is null ? null : ToDto(product);
    }

    public async Task<ProductResponseDto?> GetByBarcodeAsync(string barcode)
    {
        var product = await _repository.GetByBarcodeAsync(barcode);
        return product is null ? null : ToDto(product);
    }

    public async Task<IEnumerable<ProductResponseDto>> GetLowStockAsync() =>
        (await _repository.GetLowStockAsync()).Select(ToDto);

    public async Task<IEnumerable<ProductResponseDto>> GetByIdsAsync(IEnumerable<Guid> ids) =>
        (await _repository.GetByIdsAsync(ids)).Select(ToDto);

    public async Task<ProductResponseDto> CreateAsync(CreateProductDto dto)
    {
        var product = new Product
        {
            Barcode = dto.Barcode,
            ItemName = dto.ItemName,
            Description = dto.Description,
            CurrentStock = dto.CurrentStock,
            MinRequiredStock = dto.MinRequiredStock,
            ProviderName = dto.ProviderName
        };
        return ToDto(await _repository.CreateAsync(product));
    }

    public async Task<ImportResultDto> ImportAsync(IEnumerable<CreateProductDto> dtos)
    {
        var products = dtos.Select(dto => new Product
        {
            Barcode          = dto.Barcode,
            ItemName         = dto.ItemName,
            Description      = dto.Description ?? string.Empty,
            CurrentStock     = dto.CurrentStock,
            MinRequiredStock = dto.MinRequiredStock,
            ProviderName     = dto.ProviderName ?? string.Empty,
        });

        var (imported, skipped, skippedBarcodes) = await _repository.ImportAsync(products);
        return new ImportResultDto(imported, skipped, skippedBarcodes);
    }

    public async Task<ProductResponseDto?> UpdateAsync(Guid id, UpdateProductDto dto)
    {
        var product = await _repository.GetByIdAsync(id);
        if (product is null) return null;

        if (dto.ItemName is not null) product.ItemName = dto.ItemName;
        if (dto.Description is not null) product.Description = dto.Description;
        if (dto.MinRequiredStock is not null) product.MinRequiredStock = dto.MinRequiredStock.Value;
        if (dto.ProviderName is not null) product.ProviderName = dto.ProviderName;

        return ToDto(await _repository.UpdateAsync(product));
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var product = await _repository.GetByIdAsync(id);
        if (product is null) return false;
        await _repository.DeleteAsync(id);
        return true;
    }

    private static ProductResponseDto ToDto(Product p) => new(
        p.Id, p.Barcode, p.ItemName, p.Description,
        p.CurrentStock, p.ReservedStock, p.AvailableStock,
        p.MinRequiredStock, p.ProviderName,
        p.CurrentStock <= p.MinRequiredStock
    );
}
