namespace GestionPGB_BE.API.Application.DTOs.Auth;

public record UserDto(string Id, string UserName, string FullName, string Email, string Role);
