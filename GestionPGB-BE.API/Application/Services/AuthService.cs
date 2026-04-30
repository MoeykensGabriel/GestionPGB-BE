using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GestionPGB_BE.API.Application.DTOs.Auth;
using GestionPGB_BE.API.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace GestionPGB_BE.API.Application.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;

    public AuthService(UserManager<ApplicationUser> userManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _configuration = configuration;
    }

    public async Task<TokenResponseDto?> LoginAsync(LoginDto dto)
    {
        var user = await _userManager.FindByNameAsync(dto.UserName);
        if (user is null || !await _userManager.CheckPasswordAsync(user, dto.Password))
            return null;

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "Operator";
        var (token, expiration) = GenerateToken(user, role);

        return new TokenResponseDto(token, user.UserName!, role, expiration);
    }

    public async Task<IEnumerable<UserDto>> GetUsersAsync()
    {
        var users = _userManager.Users.OrderBy(u => u.UserName).ToList();
        var result = new List<UserDto>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            result.Add(new UserDto(
                user.Id,
                user.UserName!,
                user.FullName ?? "",
                user.Email ?? "",
                roles.FirstOrDefault() ?? "Operator"
            ));
        }
        return result;
    }

    public async Task<bool> DeleteUserAsync(string userId, string currentUserId)
    {
        if (userId == currentUserId) return false; // No puede eliminarse a sí mismo
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return false;
        var result = await _userManager.DeleteAsync(user);
        return result.Succeeded;
    }

    public async Task<bool> RegisterAsync(RegisterDto dto)
    {
        var user = new ApplicationUser
        {
            UserName = dto.UserName,
            Email = dto.Email,
            FullName = dto.FullName
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded) return false;

        var role = dto.Role == "Admin" ? "Admin" : "Operator";
        await _userManager.AddToRoleAsync(user, role);
        return true;
    }

    private (string Token, DateTime Expiration) GenerateToken(ApplicationUser user, string role)
    {
        var secret = _configuration["JWT_SECRET"]
            ?? throw new InvalidOperationException("JWT_SECRET is not configured.");

        var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret));
        var expiration = DateTime.UtcNow.AddHours(12);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName!),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiration,
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return (tokenHandler.WriteToken(token), expiration);
    }
}
