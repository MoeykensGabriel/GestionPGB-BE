using System.Security.Claims;
using GestionPGB_BE.API.Application.DTOs.Auth;
using GestionPGB_BE.API.Application.Services;

namespace GestionPGB_BE.API.API.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/login", async (LoginDto dto, IAuthService service) =>
        {
            var result = await service.LoginAsync(dto);
            return result is null ? Results.Unauthorized() : Results.Ok(result);
        })
        .WithName("Login")
        .AllowAnonymous()
        .RequireRateLimiting("login");

        group.MapPost("/register", async (RegisterDto dto, IAuthService service) =>
        {
            var success = await service.RegisterAsync(dto);
            return success
                ? Results.Created("", "Usuario registrado exitosamente.")
                : Results.BadRequest("No se pudo registrar el usuario. Verificá los datos ingresados.");
        })
        .WithName("Register")
        .RequireAuthorization(p => p.RequireRole("Admin"));

        group.MapGet("/users", async (IAuthService service) =>
            Results.Ok(await service.GetUsersAsync()))
        .WithName("GetUsers")
        .RequireAuthorization(p => p.RequireRole("Admin"));

        group.MapDelete("/users/{id}", async (string id, IAuthService service, ClaimsPrincipal currentUser) =>
        {
            var currentUserId = currentUser.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var success = await service.DeleteUserAsync(id, currentUserId);
            return success
                ? Results.NoContent()
                : Results.BadRequest("No se pudo eliminar el usuario.");
        })
        .WithName("DeleteUser")
        .RequireAuthorization(p => p.RequireRole("Admin"));
    }
}
