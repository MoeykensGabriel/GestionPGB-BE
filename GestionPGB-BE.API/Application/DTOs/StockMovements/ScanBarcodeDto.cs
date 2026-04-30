using GestionPGB_BE.API.Domain.Enums;

namespace GestionPGB_BE.API.Application.DTOs.StockMovements;

public record ScanBarcodeDto(string Barcode, MovementType Type);
