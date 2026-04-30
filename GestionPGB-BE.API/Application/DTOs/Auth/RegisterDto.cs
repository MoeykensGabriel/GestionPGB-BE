namespace GestionPGB_BE.API.Application.DTOs.Auth;

public record RegisterDto(string UserName, string Email, string Password, string FullName, string Role);
