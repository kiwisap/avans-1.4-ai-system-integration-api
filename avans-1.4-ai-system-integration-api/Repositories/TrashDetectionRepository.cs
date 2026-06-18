using avans_1._4_ai_system_integration_api.Models.Entities;
using avans_1._4_ai_system_integration_api.Repositories.Interfaces;
using avans_1_4_ai_system_integration_api.Data;

namespace avans_1._4_ai_system_integration_api.Repositories;

public class TrashDetectionRepository : ITrashDetectionRepository
{
    private readonly TrashDetectionDbContext _context;

    public TrashDetectionRepository(TrashDetectionDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(TrashDetection detection)
    {
        await _context.TrashDetections.AddAsync(detection);
        await _context.SaveChangesAsync();
    }
}