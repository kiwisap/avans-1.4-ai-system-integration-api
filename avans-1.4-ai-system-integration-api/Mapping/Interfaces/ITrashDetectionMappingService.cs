using avans_1._4_ai_system_integration_api.Models.Dtos;
using avans_1._4_ai_system_integration_api.Models.Entities;

namespace avans_1._4_ai_system_integration_api.Mapping.Interfaces;

public interface ITrashDetectionMappingService
{
    TrashDetectionDto TrashDetectionToTrashDetectionDto(TrashDetection trashDetection);
}