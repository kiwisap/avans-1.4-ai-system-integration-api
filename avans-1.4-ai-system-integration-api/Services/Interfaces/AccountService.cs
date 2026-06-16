using avans_1._4_ai_system_integration_api.Models.Entities;
using avans_1._4_ai_system_integration_api.Services.Interfaces;
using avans_1_4_ai_system_integration_api.Exceptions;
using Microsoft.AspNetCore.Identity;
using avans_1._4_ai_system_integration_api.Mapping.Interfaces;
using avans_1._4_ai_system_integration_api.Models.DTOs;
using System.Security.Claims;

namespace avans_1._4_ai_system_integration_api.Services.Interfaces;

public class AccountService : IAccountService
{
    private readonly UserManager<User> _userManager;

    private readonly IUserMappingService _userMappingService;

    public AccountService(
        UserManager<User> userManager,
        IUserMappingService userMappingService)
    {
        _userManager = userManager;
        _userMappingService = userMappingService;
    }

    public async Task<UserDTO> RegisterAsync(RegisterDTO request)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            throw new BadRequestException("Gebruiker met dit e-mailadres bestaat al.");
        }

        // DTO omzetten naar User object via de mapping service
        var user = _userMappingService.RegisterDtoToUser(request);

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            // Tijdelijk: kijk welke errors er precies zijn
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new BadRequestException(errors);
        }
        //var result = await _userManager.CreateAsync(user, request.Password);
        //if (!result.Succeeded)
        //{
        //    throw new ValidationException(result.Errors
        //        .GroupBy(e => e.Code)
        //        .ToDictionary(
        //            g => g.Key,
        //            g => g.Select(e => e.Description).ToArray()
        //        ));
        //}

        // User omzetten naar UserDTO zodat we nooit het volledige User object teruggeven
        return _userMappingService.UserToUserDto(user);
    }
    public async Task<UserDTO> LoginAsync(LoginDTO request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            throw new NotFoundException("Gebruiker niet gevonden.");
        }
        var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordValid)
        {
            throw new UnauthorizedAccessException("Ongeldig wachtwoord.");
        }
        return _userMappingService.UserToUserDto(user);
    }

    public async Task<UserDTO> GetCurrentUserAsync(ClaimsPrincipal principal)
    {
        var user = await ValidateUserExists(principal);

        return _userMappingService.UserToUserDto(user);
    }

    private async Task<User> ValidateUserExists(ClaimsPrincipal principal)
    {
        var user = await _userManager.GetUserAsync(principal);
        if (user == null)
        {
            throw new NotFoundException("Gebruiker niet gevonden.");
        }

        return user;
    }

}
