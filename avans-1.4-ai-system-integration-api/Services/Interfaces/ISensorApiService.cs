using avans_1._4_ai_system_integration_api.Models.Dtos;

namespace avans_1._4_ai_system_integration_api.Services.Interfaces;

public interface ISensorApiService
{
    Task<List<SensorTrashDataDto>> GetDetectionsAsync(DateTime startDate, DateTime endDate);
}