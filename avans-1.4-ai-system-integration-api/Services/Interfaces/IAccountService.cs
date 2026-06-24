using avans_1._4_ai_system_integration_api.Models.Dtos;
using System.Security.Claims;

namespace avans_1._4_ai_system_integration_api.Services.Interfaces;

public interface IAccountService
{
    Task<UserDto> RegisterAsync(RegisterDto request);
    Task<UserDto> GetCurrentUserAsync(ClaimsPrincipal principal);
}
