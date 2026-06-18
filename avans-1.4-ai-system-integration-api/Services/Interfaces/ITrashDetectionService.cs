using avans_1._4_ai_system_integration_api.Models.DTOs;

namespace avans_1._4_ai_system_integration_api.Services.Interfaces;

public interface ITrashDetectionService
{
    Task<List<SensorTrashDataDTO>> GetTrashDataAsync(DateTime from, DateTime to);
}