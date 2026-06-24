using avans_1._4_ai_system_integration_api.Models.Entities;

namespace avans_1._4_ai_system_integration_api.Repositories.Interfaces;

public interface ITrashDetectionRepository
{
    Task<List<TrashDetection>> GetByRangeAsync(DateTime from, DateTime to);

    Task AddRangeAsync(List<TrashDetection> detections);

    Task<TrashDataFetchLog?> FindFetchLogAsync(DateTime from, DateTime to);

    Task AddFetchLogAsync(TrashDataFetchLog log);

}