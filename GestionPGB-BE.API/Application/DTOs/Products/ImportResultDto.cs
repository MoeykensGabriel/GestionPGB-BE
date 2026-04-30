namespace GestionPGB_BE.API.Application.DTOs.Products;

public record ImportResultDto(
    int Imported,
    int Skipped,
    IEnumerable<string> SkippedBarcodes
);
