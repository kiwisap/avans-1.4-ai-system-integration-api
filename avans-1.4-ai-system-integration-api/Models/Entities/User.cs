using Microsoft.AspNetCore.Identity;

namespace avans_1._4_ai_system_integration_api.Models.Entities;

public class User : IdentityUser
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
}