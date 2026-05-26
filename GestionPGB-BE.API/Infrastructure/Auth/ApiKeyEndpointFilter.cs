namespace GestionPGB_BE.API.Infrastructure.Auth;

/// <summary>
/// Endpoint filter que valida un header X-Api-Key contra la API key del taller.
/// Usado en endpoints invocados máquina-a-máquina por sistemas externos.
/// </summary>
public class ApiKeyEndpointFilter : IEndpointFilter
{
    public const string HeaderName = "X-Api-Key";

    private readonly string _expectedKey;
    private readonly ILogger<ApiKeyEndpointFilter> _logger;

    public ApiKeyEndpointFilter(IConfiguration config, ILogger<ApiKeyEndpointFilter> logger)
    {
        _expectedKey =
            Environment.GetEnvironmentVariable("WORKSHOP_API_KEY")
            ?? config["WORKSHOP_API_KEY"]
            ?? throw new InvalidOperationException("WORKSHOP_API_KEY no está configurada.");
        _logger = logger;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext ctx, EndpointFilterDelegate next)
    {
        if (!ctx.HttpContext.Request.Headers.TryGetValue(HeaderName, out var providedKey) ||
            string.IsNullOrWhiteSpace(providedKey))
        {
            _logger.LogWarning("Solicitud rechazada: falta header {Header}", HeaderName);
            return Results.Unauthorized();
        }

        if (!string.Equals(providedKey.ToString(), _expectedKey, StringComparison.Ordinal))
        {
            _logger.LogWarning("Solicitud rechazada: API key inválida desde {Ip}",
                ctx.HttpContext.Connection.RemoteIpAddress);
            return Results.Unauthorized();
        }

        return await next(ctx);
    }
}
