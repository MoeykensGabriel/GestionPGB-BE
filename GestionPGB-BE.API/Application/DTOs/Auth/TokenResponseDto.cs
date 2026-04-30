namespace GestionPGB_BE.API.Application.DTOs.Auth;

public record TokenResponseDto(string Token, string UserName, string Role, DateTime Expiration);
