using avans_1._4_ai_system_integration_api.Models.Entities;

namespace avans_1._4_ai_system_integration_api.Services.Interfaces;

public interface ITrashDetectionService
{
    Task<List<TrashDetection>> GetTrashDataAsync(DateTime from, DateTime to);
}