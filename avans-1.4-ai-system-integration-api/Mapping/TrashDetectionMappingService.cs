using avans_1._4_ai_system_integration_api.Mapping.Interfaces;
using avans_1._4_ai_system_integration_api.Models.Dtos;
using avans_1._4_ai_system_integration_api.Models.Entities;
using avans_1._4_ai_system_integration_api.Models.Enums;

namespace avans_1._4_ai_system_integration_api.Mapping;

public class TrashDetectionMappingService : ITrashDetectionMappingService
{
    public TrashDetectionDto TrashDetectionToTrashDetectionDto(TrashDetection trashDetection)
    {
        return new TrashDetectionDto
        {
            Id = trashDetection.Id,
            SensorId = trashDetection.SensorId,
            TrashType = Enum.Parse<TrashType>(trashDetection.TrashType),
            Latitude = trashDetection.Latitude,
            Longitude = trashDetection.Longitude,
            DateTime = trashDetection.DateTime,
            Temperature = trashDetection.Temperature,
            Rain = trashDetection.Rain,
            Confidence = trashDetection.Confidence,
            ImageId = trashDetection.ImageId,
            FetchedAtUtc = trashDetection.FetchedAtUtc
        };
    }
}