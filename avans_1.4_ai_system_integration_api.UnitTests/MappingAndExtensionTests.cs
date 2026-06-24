using avans_1._4_ai_system_integration_api.Extensions;
using avans_1._4_ai_system_integration_api.Mapping;
using avans_1._4_ai_system_integration_api.Models.Dtos;
using avans_1._4_ai_system_integration_api.Models.Enums;
using TrashDetectionEntity = avans_1._4_ai_system_integration_api.Models.Entities.TrashDetection;
using UserEntity = avans_1._4_ai_system_integration_api.Models.Entities.User;

namespace Tests.Mapping;

// =============================================================================
// TrashDetectionMappingService
// =============================================================================

public class TrashDetectionMappingServiceTests
{
    private readonly TrashDetectionMappingService _sut = new();

    [Fact]
    public void TrashDetectionToDto_MapsAllFieldsCorrectly()
    {
        var id = Guid.NewGuid();
        var sensorId = Guid.NewGuid();
        var imageId = Guid.NewGuid();
        var dateTime = new DateTime(2024, 6, 15, 10, 0, 0, DateTimeKind.Utc);
        var fetchedAt = new DateTime(2024, 6, 15, 11, 0, 0, DateTimeKind.Utc);

        var entity = new TrashDetectionEntity
        {
            Id = id,
            SensorId = sensorId,
            TrashType = "Plastic",
            Latitude = 51.5f,
            Longitude = 4.3f,
            DateTime = dateTime,
            Temperature = 20f,
            Rain = 1.5f,
            Confidence = 0.88f,
            ImageId = imageId,
            FetchedAtUtc = fetchedAt
        };

        var dto = _sut.TrashDetectionToTrashDetectionDto(entity);

        Assert.Equal(id, dto.Id);
        Assert.Equal(sensorId, dto.SensorId);
        Assert.Equal(TrashType.Plastic, dto.TrashType);
        Assert.Equal(51.5f, dto.Latitude);
        Assert.Equal(4.3f, dto.Longitude);
        Assert.Equal(dateTime, dto.DateTime);
        Assert.Equal(20f, dto.Temperature);
        Assert.Equal(1.5f, dto.Rain);
        Assert.Equal(0.88f, dto.Confidence);
        Assert.Equal(imageId, dto.ImageId);
        Assert.Equal(fetchedAt, dto.FetchedAtUtc);
    }

    [Theory]
    [InlineData("Plastic", TrashType.Plastic)]
    [InlineData("Paper", TrashType.Paper)]
    [InlineData("Glass", TrashType.Glass)]
    [InlineData("Can", TrashType.Can)]
    [InlineData("Bulky_Waste", TrashType.Bulky_Waste)]
    [InlineData("Rest", TrashType.Rest)]
    public void TrashDetectionToDto_AllTrashTypes_ParsedCorrectly(string input, TrashType expected)
    {
        var entity = MakeEntity(input);

        var dto = _sut.TrashDetectionToTrashDetectionDto(entity);

        Assert.Equal(expected, dto.TrashType);
    }

    [Fact]
    public void TrashDetectionToDto_InvalidTrashType_ThrowsArgumentException()
    {
        var entity = MakeEntity("OnbekendType");

        // TrashDetectionMappingService.TrashDetectionToTrashDetectionDto is sync en gooit direct
        Assert.Throws<ArgumentException>(() =>
            _sut.TrashDetectionToTrashDetectionDto(entity));
    }

    private static TrashDetectionEntity MakeEntity(string trashType) => new()
    {
        Id = Guid.NewGuid(),
        SensorId = Guid.NewGuid(),
        TrashType = trashType,
        Latitude = 0f,
        Longitude = 0f,
        DateTime = DateTime.UtcNow,
        ImageId = Guid.NewGuid(),
        FetchedAtUtc = DateTime.UtcNow
    };
}

// =============================================================================
// UserMappingService
// =============================================================================

public class UserMappingServiceTests
{
    private readonly UserMappingService _sut = new();

    [Fact]
    public void RegisterDtoToUser_MapsNameEmailAndUserName()
    {
        var dto = new RegisterDto
        {
            Name = "Anna",
            Email = "anna@mail.nl",
            Password = "Wachtwoord1"
        };

        var user = _sut.RegisterDtoToUser(dto);

        Assert.Equal("Anna", user.Name);
        Assert.Equal("anna@mail.nl", user.Email);
        Assert.Equal("anna@mail.nl", user.UserName);
    }

    [Fact]
    public void UserToUserDto_MapsNameAndEmail()
    {
        var user = new UserEntity { Name = "Kees", Email = "kees@mail.nl" };

        var dto = _sut.UserToUserDto(user);

        Assert.Equal("Kees", dto.Name);
        Assert.Equal("kees@mail.nl", dto.Email);
    }

    [Fact]
    public void UserToUserDto_DoesNotExposePassword()
    {
        var user = new UserEntity { Name = "Kees", Email = "kees@mail.nl", PasswordHash = "geheim_hash" };

        var dto = _sut.UserToUserDto(user);

        var props = dto.GetType().GetProperties().Select(p => p.Name);
        Assert.DoesNotContain("Password", props);
        Assert.DoesNotContain("PasswordHash", props);
    }
}

// =============================================================================
// StringExtensions
// =============================================================================

public class StringExtensionsTests
{
    [Theory]
    [InlineData("plastic", "Plastic")]
    [InlineData("bulky_waste", "Bulky_Waste")]
    [InlineData("rest", "Rest")]
    [InlineData("GLASS", "GLASS")]
    [InlineData("can", "Can")]
    [InlineData("a_b_c", "A_B_C")]
    public void ToTitleCaseWithUnderscores_ConvertsCorrectly(string input, string expected)
    {
        var result = input.ToTitleCaseWithUnderscores();

        Assert.Equal(expected, result);
    }

    [Fact]
    public void ToTitleCaseWithUnderscores_EmptyString_ReturnsEmpty()
    {
        var result = "".ToTitleCaseWithUnderscores();

        Assert.Equal("", result);
    }

    [Fact]
    public void ToTitleCaseWithUnderscores_NullString_ReturnsNull()
    {
        string? input = null;

        var result = input!.ToTitleCaseWithUnderscores();

        Assert.Null(result);
    }

    [Fact]
    public void ToTitleCaseWithUnderscores_SingleChar_UpperCased()
    {
        var result = "p".ToTitleCaseWithUnderscores();

        Assert.Equal("P", result);
    }

    [Fact]
    public void ToTitleCaseWithUnderscores_BulkyWaste_CanBeParsedAsEnum()
    {
        var result = "bulky_waste".ToTitleCaseWithUnderscores();

        var success = Enum.TryParse<TrashType>(result, out var parsed);

        Assert.True(success);
        Assert.Equal(TrashType.Bulky_Waste, parsed);
    }
}