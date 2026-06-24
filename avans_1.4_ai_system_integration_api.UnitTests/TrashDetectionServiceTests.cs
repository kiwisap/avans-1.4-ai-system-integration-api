using System.ComponentModel.DataAnnotations;
using avans_1._4_ai_system_integration_api.Mapping.Interfaces;
using avans_1._4_ai_system_integration_api.Models.Dtos;
using avans_1._4_ai_system_integration_api.Models.Enums;
using avans_1._4_ai_system_integration_api.Repositories.Interfaces;
using avans_1._4_ai_system_integration_api.Services;
using avans_1._4_ai_system_integration_api.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using TrashDetectionEntity = avans_1._4_ai_system_integration_api.Models.Entities.TrashDetection;
using TrashDataFetchLogEntity = avans_1._4_ai_system_integration_api.Models.Entities.TrashDataFetchLog;

namespace Tests.Services;

public class TrashDetectionServiceTests
{
    private readonly Mock<ISensorApiService> _sensorApiServiceMock;
    private readonly Mock<ITrashDetectionMappingService> _mappingServiceMock;
    private readonly Mock<ITrashDetectionRepository> _repositoryMock;
    private readonly Mock<ILogger<TrashDetectionService>> _loggerMock;
    private readonly TrashDetectionService _sut;

    // Vaste tijdstippen zodat tests niet afhankelijk zijn van DateTime.UtcNow
    private readonly DateTime _from = new(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private readonly DateTime _to = new(2024, 1, 2, 0, 0, 0, DateTimeKind.Utc);

    public TrashDetectionServiceTests()
    {
        _sensorApiServiceMock = new Mock<ISensorApiService>();
        _mappingServiceMock = new Mock<ITrashDetectionMappingService>();
        _repositoryMock = new Mock<ITrashDetectionRepository>();
        _loggerMock = new Mock<ILogger<TrashDetectionService>>();

        _sut = new TrashDetectionService(
            _sensorApiServiceMock.Object,
            _mappingServiceMock.Object,
            _repositoryMock.Object,
            _loggerMock.Object);
    }

    // -------------------------------------------------------------------------
    // Validatie - from/to grenzen
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetTrashDataAsync_FromAfterTo_ThrowsValidationException()
    {
        var from = _to;
        var to = _from; // omgedraaid

        await Assert.ThrowsAsync<ValidationException>(() =>
            _sut.GetTrashDataAsync(from, to));
    }

    [Fact]
    public async Task GetTrashDataAsync_ToInFuture_ThrowsValidationException()
    {
        var futureDate = DateTime.UtcNow.AddHours(1);

        await Assert.ThrowsAsync<ValidationException>(() =>
            _sut.GetTrashDataAsync(_from, futureDate));
    }

    [Fact]
    public async Task GetTrashDataAsync_FromEqualsTo_ThrowsValidationException()
    {
        var same = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        await Assert.ThrowsAsync<ValidationException>(() =>
            _sut.GetTrashDataAsync(same, same));
    }

    // -------------------------------------------------------------------------
    // Cache - verse data (fetch log jonger dan 30 minuten)
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetTrashDataAsync_FreshFetchLog_ReturnsDatabaseDataWithoutCallingApi()
    {
        var freshLog = new TrashDataFetchLogEntity
        {
            RangeFrom = _from,
            RangeTo = _to,
            FetchedAtUtc = DateTime.UtcNow.AddMinutes(-10) // vers
        };

        var dbEntities = new List<TrashDetectionEntity>
        {
            MakeTrashDetection(TrashType.Plastic)
        };

        _repositoryMock.Setup(r => r.FindFetchLogAsync(_from, _to))
            .ReturnsAsync(freshLog);

        _repositoryMock.Setup(r => r.GetByRangeAsync(_from, _to))
            .ReturnsAsync(dbEntities);

        _mappingServiceMock.Setup(m => m.TrashDetectionToTrashDetectionDto(It.IsAny<TrashDetectionEntity>()))
            .Returns(new TrashDetectionDto());

        var result = await _sut.GetTrashDataAsync(_from, _to);

        Assert.Single(result);
        _sensorApiServiceMock.Verify(s => s.GetDetectionsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never);
    }

    [Fact]
    public async Task GetTrashDataAsync_FreshFetchLog_DoesNotSaveNewFetchLog()
    {
        var freshLog = new TrashDataFetchLogEntity
        {
            FetchedAtUtc = DateTime.UtcNow.AddMinutes(-5)
        };

        _repositoryMock.Setup(r => r.FindFetchLogAsync(_from, _to)).ReturnsAsync(freshLog);
        _repositoryMock.Setup(r => r.GetByRangeAsync(_from, _to)).ReturnsAsync([]);

        await _sut.GetTrashDataAsync(_from, _to);

        _repositoryMock.Verify(r => r.AddFetchLogAsync(It.IsAny<TrashDataFetchLogEntity>()), Times.Never);
    }

    // -------------------------------------------------------------------------
    // Cache - verlopen data (fetch log ouder dan 30 minuten, of geen log)
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetTrashDataAsync_NoFetchLog_CallsSensorApi()
    {
        _repositoryMock.Setup(r => r.FindFetchLogAsync(_from, _to)).ReturnsAsync((TrashDataFetchLogEntity?)null);
        _sensorApiServiceMock.Setup(s => s.GetDetectionsAsync(_from, _to)).ReturnsAsync([]);

        await _sut.GetTrashDataAsync(_from, _to);

        _sensorApiServiceMock.Verify(s => s.GetDetectionsAsync(_from, _to), Times.Once);
    }

    [Fact]
    public async Task GetTrashDataAsync_StaleFetchLog_CallsSensorApi()
    {
        var staleLog = new TrashDataFetchLogEntity
        {
            FetchedAtUtc = DateTime.UtcNow.AddMinutes(-31) // ouder dan 30 minuten
        };

        _repositoryMock.Setup(r => r.FindFetchLogAsync(_from, _to)).ReturnsAsync(staleLog);
        _sensorApiServiceMock.Setup(s => s.GetDetectionsAsync(_from, _to)).ReturnsAsync([]);

        await _sut.GetTrashDataAsync(_from, _to);

        _sensorApiServiceMock.Verify(s => s.GetDetectionsAsync(_from, _to), Times.Once);
    }

    [Fact]
    public async Task GetTrashDataAsync_ApiReturnsData_SavesEntitiesAndFetchLog()
    {
        _repositoryMock.Setup(r => r.FindFetchLogAsync(_from, _to)).ReturnsAsync((TrashDataFetchLogEntity?)null);

        var sensorDtos = new List<SensorTrashDataDto>
        {
            MakeSensorDto("Plastic")
        };

        _sensorApiServiceMock.Setup(s => s.GetDetectionsAsync(_from, _to)).ReturnsAsync(sensorDtos);
        _mappingServiceMock.Setup(m => m.TrashDetectionToTrashDetectionDto(It.IsAny<TrashDetectionEntity>()))
            .Returns(new TrashDetectionDto());

        await _sut.GetTrashDataAsync(_from, _to);

        _repositoryMock.Verify(r => r.AddRangeAsync(It.Is<List<TrashDetectionEntity>>(l => l.Count == 1)), Times.Once);
        _repositoryMock.Verify(r => r.AddFetchLogAsync(It.IsAny<TrashDataFetchLogEntity>()), Times.Once);
    }

    // -------------------------------------------------------------------------
    // Validatie van sensor data (TryValidateAndMap)
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("Plastic")]
    [InlineData("plastic")]
    [InlineData("PLASTIC")]
    [InlineData("bulky_waste")]
    [InlineData("Bulky_Waste")]
    [InlineData("Rest")]
    [InlineData("Glass")]
    [InlineData("Can")]
    [InlineData("Paper")]
    public async Task GetTrashDataAsync_ValidTrashTypes_AreIncludedInResult(string trashType)
    {
        _repositoryMock.Setup(r => r.FindFetchLogAsync(_from, _to)).ReturnsAsync((TrashDataFetchLogEntity?)null);

        _sensorApiServiceMock.Setup(s => s.GetDetectionsAsync(_from, _to))
            .ReturnsAsync([MakeSensorDto(trashType)]);

        _mappingServiceMock.Setup(m => m.TrashDetectionToTrashDetectionDto(It.IsAny<TrashDetectionEntity>()))
            .Returns(new TrashDetectionDto());

        await _sut.GetTrashDataAsync(_from, _to);

        _repositoryMock.Verify(r => r.AddRangeAsync(It.Is<List<TrashDetectionEntity>>(l => l.Count == 1)), Times.Once);
    }

    [Fact]
    public async Task GetTrashDataAsync_UnknownTrashType_IsSkipped()
    {
        _repositoryMock.Setup(r => r.FindFetchLogAsync(_from, _to)).ReturnsAsync((TrashDataFetchLogEntity?)null);

        _sensorApiServiceMock.Setup(s => s.GetDetectionsAsync(_from, _to))
            .ReturnsAsync([MakeSensorDto("Onbekend_Type_XYZ")]);

        await _sut.GetTrashDataAsync(_from, _to);

        _repositoryMock.Verify(r => r.AddRangeAsync(It.Is<List<TrashDetectionEntity>>(l => l.Count == 0)), Times.Once);
    }

    [Theory]
    [InlineData(-91f)]
    [InlineData(91f)]
    public async Task GetTrashDataAsync_LatitudeOutOfRange_IsSkipped(float latitude)
    {
        _repositoryMock.Setup(r => r.FindFetchLogAsync(_from, _to)).ReturnsAsync((TrashDataFetchLogEntity?)null);

        var dto = MakeSensorDto("Plastic");
        dto.Latitude = latitude;

        _sensorApiServiceMock.Setup(s => s.GetDetectionsAsync(_from, _to)).ReturnsAsync([dto]);

        await _sut.GetTrashDataAsync(_from, _to);

        _repositoryMock.Verify(r => r.AddRangeAsync(It.Is<List<TrashDetectionEntity>>(l => l.Count == 0)), Times.Once);
    }

    [Theory]
    [InlineData(-181f)]
    [InlineData(181f)]
    public async Task GetTrashDataAsync_LongitudeOutOfRange_IsSkipped(float longitude)
    {
        _repositoryMock.Setup(r => r.FindFetchLogAsync(_from, _to)).ReturnsAsync((TrashDataFetchLogEntity?)null);

        var dto = MakeSensorDto("Plastic");
        dto.Longitude = longitude;

        _sensorApiServiceMock.Setup(s => s.GetDetectionsAsync(_from, _to)).ReturnsAsync([dto]);

        await _sut.GetTrashDataAsync(_from, _to);

        _repositoryMock.Verify(r => r.AddRangeAsync(It.Is<List<TrashDetectionEntity>>(l => l.Count == 0)), Times.Once);
    }

    [Fact]
    public async Task GetTrashDataAsync_DateTimeInFuture_IsSkipped()
    {
        _repositoryMock.Setup(r => r.FindFetchLogAsync(_from, _to)).ReturnsAsync((TrashDataFetchLogEntity?)null);

        var dto = MakeSensorDto("Plastic");
        dto.DateTime = DateTime.UtcNow.AddHours(1);

        _sensorApiServiceMock.Setup(s => s.GetDetectionsAsync(_from, _to)).ReturnsAsync([dto]);

        await _sut.GetTrashDataAsync(_from, _to);

        _repositoryMock.Verify(r => r.AddRangeAsync(It.Is<List<TrashDetectionEntity>>(l => l.Count == 0)), Times.Once);
    }

    [Fact]
    public async Task GetTrashDataAsync_MixedValidAndInvalidDtos_OnlyValidAreSaved()
    {
        _repositoryMock.Setup(r => r.FindFetchLogAsync(_from, _to)).ReturnsAsync((TrashDataFetchLogEntity?)null);

        var valid = MakeSensorDto("Plastic");
        var invalidType = MakeSensorDto("NietBestaand");
        var invalidLat = MakeSensorDto("Glass");
        invalidLat.Latitude = 999f;

        _sensorApiServiceMock.Setup(s => s.GetDetectionsAsync(_from, _to))
            .ReturnsAsync([valid, invalidType, invalidLat]);

        _mappingServiceMock.Setup(m => m.TrashDetectionToTrashDetectionDto(It.IsAny<TrashDetectionEntity>()))
            .Returns(new TrashDetectionDto());

        await _sut.GetTrashDataAsync(_from, _to);

        _repositoryMock.Verify(r => r.AddRangeAsync(It.Is<List<TrashDetectionEntity>>(l => l.Count == 1)), Times.Once);
    }

    // -------------------------------------------------------------------------
    // Mapping van TrashDetection entity (via TryValidateAndMap)
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetTrashDataAsync_ValidDto_MapsAllFieldsCorrectly()
    {
        _repositoryMock.Setup(r => r.FindFetchLogAsync(_from, _to)).ReturnsAsync((TrashDataFetchLogEntity?)null);

        var sensorId = Guid.NewGuid();
        var imageId = Guid.NewGuid();
        var detectionTime = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        var dto = new SensorTrashDataDto
        {
            Id = sensorId,
            TrashType = "Glass",
            Latitude = 51.5f,
            Longitude = 4.3f,
            DateTime = detectionTime,
            Temperature = 18.5f,
            Rain = 0.0f,
            Confidence = 0.95f,
            ImageId = imageId
        };

        _sensorApiServiceMock.Setup(s => s.GetDetectionsAsync(_from, _to)).ReturnsAsync([dto]);
        _mappingServiceMock.Setup(m => m.TrashDetectionToTrashDetectionDto(It.IsAny<TrashDetectionEntity>()))
            .Returns(new TrashDetectionDto());

        await _sut.GetTrashDataAsync(_from, _to);

        _repositoryMock.Verify(r => r.AddRangeAsync(It.Is<List<TrashDetectionEntity>>(entities =>
            entities[0].SensorId == sensorId &&
            entities[0].TrashType == "Glass" &&
            entities[0].Latitude == 51.5f &&
            entities[0].Longitude == 4.3f &&
            entities[0].DateTime == detectionTime &&
            entities[0].Temperature == 18.5f &&
            entities[0].Rain == 0.0f &&
            entities[0].Confidence == 0.95f &&
            entities[0].ImageId == imageId
        )), Times.Once);
    }

    // -------------------------------------------------------------------------
    // Lege API response
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetTrashDataAsync_ApiReturnsEmptyList_ReturnsEmptyResult()
    {
        _repositoryMock.Setup(r => r.FindFetchLogAsync(_from, _to)).ReturnsAsync((TrashDataFetchLogEntity?)null);
        _sensorApiServiceMock.Setup(s => s.GetDetectionsAsync(_from, _to)).ReturnsAsync([]);

        var result = await _sut.GetTrashDataAsync(_from, _to);

        Assert.Empty(result);
        _repositoryMock.Verify(r => r.AddRangeAsync(It.Is<List<TrashDetectionEntity>>(l => l.Count == 0)), Times.Once);
        _repositoryMock.Verify(r => r.AddFetchLogAsync(It.IsAny<TrashDataFetchLogEntity>()), Times.Once);
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static SensorTrashDataDto MakeSensorDto(string trashType) => new()
    {
        Id = Guid.NewGuid(),
        TrashType = trashType,
        Latitude = 51.5f,
        Longitude = 4.3f,
        DateTime = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc),
        Temperature = 15f,
        Rain = 0f,
        Confidence = 0.9f,
        ImageId = Guid.NewGuid()
    };

    private static TrashDetectionEntity MakeTrashDetection(TrashType type) => new()
    {
        Id = Guid.NewGuid(),
        SensorId = Guid.NewGuid(),
        TrashType = type.ToString(),
        Latitude = 51.5f,
        Longitude = 4.3f,
        DateTime = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc),
        FetchedAtUtc = DateTime.UtcNow
    };
}