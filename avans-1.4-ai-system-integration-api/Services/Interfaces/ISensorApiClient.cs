using avans_1._4_ai_system_integration_api.Models.DTOs;

namespace avans_1._4_ai_system_integration_api.Services.Interfaces;

public interface ISensorApiClient
{
    Task<List<SensorTrashDataDTO>> GetLatestDetectionsAsync(DateTime from, DateTime to);
}