using GestionPGB_BE.API.Application.DTOs.Products;

namespace GestionPGB_BE.API.Application.Services;

public interface IPdfService
{
    byte[] GenerateQuotationPdf(IEnumerable<QuotationPdfItemDto> items);
}
