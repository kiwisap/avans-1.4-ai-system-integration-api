using Microsoft.AspNetCore.Identity;

namespace avans_1._4_ai_system_integration_api.Models.Entities;

public class User : IdentityUser
{
    public string Name { get; set; } = string.Empty;
}