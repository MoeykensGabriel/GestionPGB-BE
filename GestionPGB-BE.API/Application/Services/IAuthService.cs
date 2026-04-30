using GestionPGB_BE.API.Application.DTOs.Auth;

namespace GestionPGB_BE.API.Application.Services;

public interface IAuthService
{
    Task<TokenResponseDto?> LoginAsync(LoginDto dto);
    Task<bool> RegisterAsync(RegisterDto dto);
    Task<IEnumerable<UserDto>> GetUsersAsync();
    Task<bool> DeleteUserAsync(string userId, string currentUserId);
}
