using GestionPGB_BE.API.Application.DTOs.Products;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace GestionPGB_BE.API.Application.Services;

public class PdfService : IPdfService
{
    public byte[] GenerateQuotationPdf(IEnumerable<QuotationPdfItemDto> items)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var grouped = items
            .GroupBy(i => i.Product.ProviderName)
            .OrderBy(g => g.Key)
            .ToList();

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header()
                    .PaddingBottom(10)
                    .Column(col =>
                    {
                        col.Item().Text("Pedido de Cotización")
                            .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);
                        col.Item().Text($"Fecha: {DateTime.Now:dd/MM/yyyy}")
                            .FontSize(10).FontColor(Colors.Grey.Medium);
                    });

                page.Content().Column(col =>
                {
                    foreach (var group in grouped)
                    {
                        col.Item().PaddingTop(15).Text($"Proveedor: {group.Key}")
                            .Bold().FontSize(13);

                        col.Item().PaddingTop(5).Table(table =>
                        {
                            table.ColumnsDefinition(cols =>
                            {
                                cols.RelativeColumn(3);
                                cols.RelativeColumn(5);
                                cols.RelativeColumn(2);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Grey.Lighten3)
                                    .Padding(5).Text("Ítem").Bold();
                                header.Cell().Background(Colors.Grey.Lighten3)
                                    .Padding(5).Text("Descripción").Bold();
                                header.Cell().Background(Colors.Grey.Lighten3)
                                    .Padding(5).AlignRight().Text("Cantidad").Bold();
                            });

                            foreach (var item in group)
                            {
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                    .Padding(5).Text(item.Product.ItemName);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                    .Padding(5).Text(item.Product.Description);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                    .Padding(5).AlignRight().Text(item.Quantity.ToString());
                            }
                        });
                    }
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Página ");
                    x.CurrentPageNumber();
                    x.Span(" de ");
                    x.TotalPages();
                });
            });
        }).GeneratePdf();
    }
}
