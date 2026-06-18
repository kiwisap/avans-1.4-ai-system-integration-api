using avans_1._4_ai_system_integration_api.Models.Entities;
namespace avans_1._4_ai_system_integration_api.Repositories.Interfaces;

public interface ITrashDetectionRepository
{
    Task AddAsync(TrashDetection trashDetection);
}
