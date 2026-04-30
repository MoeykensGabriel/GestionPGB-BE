namespace GestionPGB_BE.API.Application.DTOs;

public record PagedResultDto<T>(
    IEnumerable<T> Items,
    int Total,
    int Page,
    int PageSize
);
