using avans_1._4_ai_system_integration_api.Models.DTOs;
using System.Security.Claims;

namespace avans_1._4_ai_system_integration_api.Services.Interfaces;

public interface IAccountService
{
    Task<UserDTO> RegisterAsync(RegisterDTO request);
    Task<UserDTO> GetCurrentUserAsync(ClaimsPrincipal principal);
}
