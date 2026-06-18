using avans_1._4_ai_system_integration_api.Models.DTOs;
using avans_1._4_ai_system_integration_api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace avans_1._4_ai_system_integration_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAccountService _accountService;

    public AuthController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    // POST api/auth/register
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterDTO request)
    {
        var user = await _accountService.RegisterAsync(request);
        return Ok(user);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me()
    {
        var user = await _accountService.GetCurrentUserAsync(User);
        return Ok(user);
    }
}
