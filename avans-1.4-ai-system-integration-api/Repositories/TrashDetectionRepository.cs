using avans_1._4_ai_system_integration_api.Models.Entities;
using avans_1._4_ai_system_integration_api.Repositories.Interfaces;
using avans_1_4_ai_system_integration_api.Data;
using Microsoft.EntityFrameworkCore;

namespace avans_1._4_ai_system_integration_api.Repositories;

public class TrashDetectionRepository : ITrashDetectionRepository
{
    private readonly TrashDetectionDbContext _context;

    public TrashDetectionRepository(TrashDetectionDbContext context)
    {
        _context = context;
    }

    public async Task<List<TrashDetection>> GetByRangeAsync(DateTime from, DateTime to)
    {
        return await _context.TrashDetections
            .Where(t => t.PhotoTakenAtUtc >= from && t.PhotoTakenAtUtc <= to)
            .ToListAsync();
    }

    // controleert per detectie of er al een record bestaat met dezelfde camera locatie en tijdstip, zo niet dan wordt deze toegevoegd
    public async Task AddRangeAsync(List<TrashDetection> detections)
    {
        foreach (var detection in detections)
        {
            var alreadyExists = await _context.TrashDetections.AnyAsync(t =>
                t.CameraLatitude == detection.CameraLatitude &&
                t.CameraLongitude == detection.CameraLongitude &&
                t.PhotoTakenAtUtc == detection.PhotoTakenAtUtc);

            if (!alreadyExists)
            {
                await _context.TrashDetections.AddAsync(detection);
            }
        }
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
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
        await _context.TrashDataFetchLogs.AddAsync(log);
    }
}