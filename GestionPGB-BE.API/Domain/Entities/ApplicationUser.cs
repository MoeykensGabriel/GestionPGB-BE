using Microsoft.AspNetCore.Identity;

namespace GestionPGB_BE.API.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
}
