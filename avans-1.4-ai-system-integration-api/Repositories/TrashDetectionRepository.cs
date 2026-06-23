using avans_1._4_ai_system_integration_api.Models.Entities;
using avans_1._4_ai_system_integration_api.Repositories.Interfaces;
using avans_1_4_ai_system_integration_api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace avans_1._4_ai_system_integration_api.Repositories;
public class TrashDetectionRepository : ITrashDetectionRepository
{
    private readonly TrashDetectionDbContext _context;
    private readonly ILogger<TrashDetectionRepository> _logger;

    public TrashDetectionRepository(TrashDetectionDbContext context, ILogger<TrashDetectionRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<TrashDetection>> GetByRangeAsync(DateTime from, DateTime to)
    {
        return await _context.TrashDetections
            .Where(t => t.DateTime >= from && t.DateTime <= to)
            .ToListAsync();
    }

    // controleert per detectie of er al een record bestaat met dezelfde camera locatie en tijdstip, zo niet dan wordt deze toegevoegd
    public async Task AddRangeAsync(List<TrashDetection> detections)
    {
        foreach (var detection in detections)
        {
            try
            {
                _context.TrashDetections.Add(detection);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogWarning(ex, "Dubbele TrashDetection overgeslagen: camera ({Lat}, {Lng}) op {Time}",
                    detection.Latitude, detection.Longitude, detection.DateTime);
                // zorgt ervoor dat de entity niet in de context blijft hangen
                _context.Entry(detection).State = EntityState.Detached;
            }
        }
    }


    // nodig om te checken of er al een fetch log is voor de opgegeven range
    public async Task<TrashDataFetchLog?> FindFetchLogAsync(DateTime from, DateTime to)
    {
        return await _context.TrashDataFetchLogs
            .Where(l => l.RangeFrom == from && l.RangeTo == to)
            .OrderByDescending(l => l.FetchedAtUtc)
            .FirstOrDefaultAsync();
    }
    public async Task AddFetchLogAsync(TrashDataFetchLog log)
    {
        _context.TrashDataFetchLogs.Add(log);
    }
    public Task SaveChangesAsync()
    {
        throw new NotImplementedException();
    }
}