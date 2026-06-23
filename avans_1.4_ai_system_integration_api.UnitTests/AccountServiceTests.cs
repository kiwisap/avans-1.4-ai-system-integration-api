// avans_1.4_ai_system_integration_api.UnitTests/Services/AccountServiceTests.cs
using avans_1._4_ai_system_integration_api.Mapping.Interfaces;
using avans_1._4_ai_system_integration_api.Models.DTOs;
using avans_1._4_ai_system_integration_api.Models.Entities;
using avans_1._4_ai_system_integration_api.Services.Interfaces;
using avans_1_4_ai_system_integration_api.Exceptions;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;

namespace avans_1._4_ai_system_integration_api.UnitTests.Services;

public class AccountServiceTests
{
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<IUserMappingService> _mappingServiceMock;
    private readonly AccountService _service;

    public AccountServiceTests()
    {
        // UserManager heeft een ingewikkelde constructor, vandaar deze "nep store"
        var storeMock = new Mock<IUserStore<User>>();
        _userManagerMock = new Mock<UserManager<User>>(
            storeMock.Object, null, null, null, null, null, null, null, null);

        _mappingServiceMock = new Mock<IUserMappingService>();

        _service = new AccountService(_userManagerMock.Object, _mappingServiceMock.Object);
    }

    [Fact]
    public async Task RegisterAsync_WhenEmailAlreadyExists_ThrowsBadRequestException()
    {
        // Arrange
        var request = new RegisterDTO { Email = "bestaat@al.nl", Password = "Test1234!" };
        var existingUser = new User { Email = request.Email };

        _userManagerMock
            .Setup(m => m.FindByEmailAsync(request.Email))
            .ReturnsAsync(existingUser);

        // Act + Assert
        await Assert.ThrowsAsync<BadRequestException>(
            () => _service.RegisterAsync(request));
    }

    [Fact]
    public async Task RegisterAsync_WhenEmailIsNew_CreatesUserSuccessfully()
    {
        // Arrange
        var request = new RegisterDTO { Email = "nieuw@test.nl", Password = "Test1234!" };
        var mappedUser = new User { Email = request.Email, UserName = request.Email };

        _userManagerMock
            .Setup(m => m.FindByEmailAsync(request.Email))
            .ReturnsAsync((User?)null); // bestaat nog niet

        _mappingServiceMock
            .Setup(m => m.RegisterDtoToUser(request))
            .Returns(mappedUser);

        _userManagerMock
            .Setup(m => m.CreateAsync(mappedUser, request.Password))
            .ReturnsAsync(IdentityResult.Success);

        _mappingServiceMock
            .Setup(m => m.UserToUserDto(mappedUser))
            .Returns(new UserDTO { Email = request.Email });

        // Act
        var result = await _service.RegisterAsync(request);

        // Assert
        Assert.Equal(request.Email, result.Email);
        _userManagerMock.Verify(m => m.CreateAsync(mappedUser, request.Password), Times.Once);
    }
}