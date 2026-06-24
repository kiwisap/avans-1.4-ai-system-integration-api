using avans_1._4_ai_system_integration_api.Mapping.Interfaces;
using avans_1._4_ai_system_integration_api.Models.Dtos;
using avans_1._4_ai_system_integration_api.Models.Entities;
using avans_1._4_ai_system_integration_api.Services;
using avans_1_4_ai_system_integration_api.Exceptions;
using Microsoft.AspNetCore.Identity;
using Moq;
using System.Security.Claims;

namespace TrashDetection.Tests.Services;

public class AccountServiceTests
{
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<IUserMappingService> _userMappingServiceMock;
    private readonly AccountService _sut;

    public AccountServiceTests()
    {
        // UserManager heeft een IUserStore nodig als minimale dependency
        var storeMock = new Mock<IUserStore<User>>();
        _userManagerMock = new Mock<UserManager<User>>(
            storeMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _userMappingServiceMock = new Mock<IUserMappingService>();

        _sut = new AccountService(_userManagerMock.Object, _userMappingServiceMock.Object);
    }

    // -------------------------------------------------------------------------
    // RegisterAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task RegisterAsync_NewEmail_CreatesUserAndReturnsDto()
    {
        var request = new RegisterDto { Name = "Jan", Email = "jan@mail.nl", Password = "Wachtwoord1" };
        var userEntity = new User { Name = "Jan", Email = "jan@mail.nl" };
        var expectedDto = new UserDto { Name = "Jan", Email = "jan@mail.nl" };

        _userManagerMock.Setup(u => u.FindByEmailAsync(request.Email)).ReturnsAsync((User?)null);
        _userMappingServiceMock.Setup(m => m.RegisterDtoToUser(request)).Returns(userEntity);
        _userManagerMock.Setup(u => u.CreateAsync(userEntity, request.Password))
            .ReturnsAsync(IdentityResult.Success);
        _userMappingServiceMock.Setup(m => m.UserToUserDto(userEntity)).Returns(expectedDto);

        var result = await _sut.RegisterAsync(request);

        Assert.Equal("Jan", result.Name);
        Assert.Equal("jan@mail.nl", result.Email);
    }

    [Fact]
    public async Task RegisterAsync_ExistingEmail_ThrowsBadRequestException()
    {
        var request = new RegisterDto { Email = "bestaat@mail.nl" };

        _userManagerMock.Setup(u => u.FindByEmailAsync(request.Email))
            .ReturnsAsync(new User());

        await Assert.ThrowsAsync<BadRequestException>(() => _sut.RegisterAsync(request));
    }

    [Fact]
    public async Task RegisterAsync_IdentityFailure_ThrowsValidationException()
    {
        var request = new RegisterDto { Name = "Test", Email = "test@mail.nl", Password = "te_zwak" };
        var userEntity = new User();

        _userManagerMock.Setup(u => u.FindByEmailAsync(request.Email)).ReturnsAsync((User?)null);
        _userMappingServiceMock.Setup(m => m.RegisterDtoToUser(request)).Returns(userEntity);
        _userManagerMock.Setup(u => u.CreateAsync(userEntity, request.Password))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError
            {
                Code = "PasswordTooShort",
                Description = "Wachtwoord te kort."
            }));

        await Assert.ThrowsAsync<ValidationException>(() => _sut.RegisterAsync(request));
    }

    // -------------------------------------------------------------------------
    // GetCurrentUserAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetCurrentUserAsync_KnownUser_ReturnsUserDto()
    {
        var user = new User { Name = "Piet", Email = "piet@mail.nl" };
        var expectedDto = new UserDto { Name = "Piet", Email = "piet@mail.nl" };
        var principal = new ClaimsPrincipal();

        _userManagerMock.Setup(u => u.GetUserAsync(principal)).ReturnsAsync(user);
        _userMappingServiceMock.Setup(m => m.UserToUserDto(user)).Returns(expectedDto);

        var result = await _sut.GetCurrentUserAsync(principal);

        Assert.Equal("Piet", result.Name);
    }

    [Fact]
    public async Task GetCurrentUserAsync_UnknownUser_ThrowsNotFoundException()
    {
        var principal = new ClaimsPrincipal();

        _userManagerMock.Setup(u => u.GetUserAsync(principal)).ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetCurrentUserAsync(principal));
    }
}