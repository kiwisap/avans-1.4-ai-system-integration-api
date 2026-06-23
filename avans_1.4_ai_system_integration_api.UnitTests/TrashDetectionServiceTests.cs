using avans_1._4_ai_system_integration_api.Models.DTOs;
using avans_1._4_ai_system_integration_api.Models.Entities;
using avans_1._4_ai_system_integration_api.Repositories.Interfaces;
using avans_1._4_ai_system_integration_api.Services;
using avans_1._4_ai_system_integration_api.Services.Interfaces;
using avans_1._4_ai_system_integration_api.Models.Enums;
using avans_1_4_ai_system_integration_api.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;
using System.ComponentModel.DataAnnotations;
using Xunit;
using ValidationException = System.ComponentModel.DataAnnotations.ValidationException;

namespace avans_1._4_ai_system_integration_api.UnitTests.Services
{
    public class TrashDetectionServiceTests
    {
        private readonly Mock<ISensorApiClient> _sensorApiClientMock;
        private readonly Mock<ITrashDetectionRepository> _repositoryMock;
        private readonly Mock<ILogger<TrashDetectionService>> _loggerMock;
        private readonly TrashDetectionService _service;

        public TrashDetectionServiceTests()
        {
            _sensorApiClientMock = new Mock<ISensorApiClient>();
            _repositoryMock = new Mock<ITrashDetectionRepository>();
            _loggerMock = new Mock<ILogger<TrashDetectionService>>();

            _service = new TrashDetectionService(
                _sensorApiClientMock.Object,
                _repositoryMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task GetTrashDataAsync_WhenFromIsAfterTo_ThrowsValidationException()
        {
            // Arrange
            var from = DateTime.UtcNow;
            var to = DateTime.UtcNow.AddDays(-1); // to ligt vóór from

            // Act + Assert
            await Assert.ThrowsAsync<ValidationException>(
                () => _service.GetTrashDataAsync(from, to));
        }

        [Fact]
        public async Task GetTrashDataAsync_WhenToIsInTheFuture_ThrowsValidationException()
        {
            // Arrange
            var from = DateTime.UtcNow.AddDays(-1);
            var to = DateTime.UtcNow.AddDays(1); // to ligt in de toekomst

            // Act + Assert
            await Assert.ThrowsAsync<ValidationException>(
                () => _service.GetTrashDataAsync(from, to));
        }

        [Fact]
        public async Task GetTrashDataAsync_WhenDataAlreadyInDatabase_DoesNotCallSensorApi()
        {
            // Arrange
            var from = DateTime.UtcNow.AddDays(-2);
            var to = DateTime.UtcNow.AddDays(-1);
            
            var existingData = new List<TrashDetection>
            {
                new TrashDetection { Id = Guid.NewGuid(), DateTime = from.AddHours(1) }
            };

            _repositoryMock
                .Setup(r => r.GetByRangeAsync(from, to))
                .ReturnsAsync(existingData);

            // Act
            var result = await _service.GetTrashDataAsync(from, to);

            // Assert
            Assert.Equal(existingData, result);
            _sensorApiClientMock.Verify(
                client => client.GetLatestDetectionsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()),
                Times.Never); // de sensoring-API mag NIET aangeroepen zijn
        }

        [Fact]
        public async Task GetTrashDataAsync_WhenNoDataInDatabase_FetchesFromSensorApi()
        {
            // Arrange
            var from = DateTime.UtcNow.AddDays(-2);
            var to = DateTime.UtcNow.AddDays(-1);

            _repositoryMock
                .Setup(r => r.GetByRangeAsync(from, to))
                .ReturnsAsync(new List<TrashDetection>()); // database is leeg

            var sensorData = new List<SensorTrashDataDTO>
            {
                new SensorTrashDataDTO
                {
                    Latitude = 51,
                    Longitude = 4,
                    DateTime = from.AddHours(1),
                    Temperature = 18,
                    TrashType = "PLASTIC",

                }
            };

            _sensorApiClientMock
                .Setup(c => c.GetLatestDetectionsAsync(from, to))
                .ReturnsAsync(sensorData);

            // Act
            var result = await _service.GetTrashDataAsync(from, to);

            // Assert
            Assert.Single(result);
            Assert.Equal("PLASTIC", result[0].TrashType);
            _repositoryMock.Verify(r => r.AddRangeAsync(It.IsAny<List<TrashDetection>>()), Times.Once);
        }
    }
}