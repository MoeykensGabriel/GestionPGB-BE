using System.Net;
using System.Text.Json;

namespace GestionPGB_BE.API.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Excepción no controlada: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title) = exception switch
        {
            KeyNotFoundException => (HttpStatusCode.NotFound, "Recurso no encontrado"),
            UnauthorizedAccessException => (HttpStatusCode.Forbidden, "Acceso denegado"),
            ArgumentException => (HttpStatusCode.BadRequest, "Datos inválidos"),
            InvalidOperationException => (HttpStatusCode.BadRequest, "Operación inválida"),
            _ => (HttpStatusCode.InternalServerError, "Error interno del servidor")
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            status = (int)statusCode,
            title,
            detail = exception.Message,
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
