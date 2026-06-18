using avans_1._4_ai_system_integration_api.Models.DTOs;
using avans_1._4_ai_system_integration_api.Models.Entities;
using avans_1._4_ai_system_integration_api.Repositories.Interfaces;
using avans_1._4_ai_system_integration_api.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace avans_1._4_ai_system_integration_api.Services;

public class TrashDetectionService : ITrashDetectionService
{
    private readonly ISensorApiClient _sensorApiClient;
    private readonly ITrashDetectionRepository _repository;
    private readonly ILogger<TrashDetectionService> _logger;

    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);

    public TrashDetectionService(
        ISensorApiClient sensorApiClient,
        ITrashDetectionRepository repository,
        ILogger<TrashDetectionService> logger)
    {
        _sensorApiClient = sensorApiClient;
        _repository = repository;
        _logger = logger;
    }

    public async Task<List<TrashDetection>> GetTrashDataAsync(DateTime from, DateTime to)
    {
        var fetchLog = await _repository.FindFetchLogAsync(from, to);
        var isFresh = fetchLog != null && DateTime.UtcNow - fetchLog.FetchedAtUtc < CacheDuration;

        if (isFresh)
        {
            _logger.LogInformation("Data voor range {From} - {To} is nog vers, ophalen uit database", from, to);
            return await _repository.GetByRangeAsync(from, to);
        }

        _logger.LogInformation("Data voor range {From} - {To} is niet vers, ophalen van API", from, to);

        var sensorData = await _sensorApiClient.GetLatestDetectionsAsync(from, to);
        var entities = MapAndValidate(sensorData);

        await _repository.AddRangeAsync(entities);
        await _repository.AddFetchLogAsync(new TrashDataFetchLog
        {
            RangeFrom = from,
            RangeTo = to,
            FetchedAtUtc = DateTime.UtcNow
        });

        await _repository.SaveChangesAsync();

        return entities;
    }

    private List<TrashDetection> MapAndValidate(List<SensorTrashDataDTO> dTOs)
    {
        var result = new List<TrashDetection>();

        foreach (var dto in dTOs)
        {
            if (!TryValidateAndMap(dto, out var entity, out var error))
            {
                _logger.LogWarning("Ongeldige data gedetecteerd: {Error}", error);
                continue;
            }
            result.Add(entity);
        }
        return result;
    }

    private bool TryValidateAndMap(SensorTrashDataDTO dto, out TrashDetection? entity, out string? error)
    {
        entity = null;
        
        if (!Enum.TryParse<TrashType>(dto.Type, true, out var trashType))
        {
            error = $"Onbekend afvaltype: {dto.Type}";
            return false;
        }
        if (dto.CameraLatitude < -90 || dto.CameraLatitude > 90)
        {
            error = "Latitude buiten geldig bereik (-90 tot 90)";
            return false;
        }

        if (dto.CameraLongitude < -180 || dto.CameraLongitude > 180)
        {
            error = "Longitude buiten geldig bereik (-180 tot 180)";
            return false;
        }
        if (dto.PhotoTakenAtUtc > DateTime.UtcNow)
        {
            error = "Tijdstip foto ligt in de toekomst";
            return false;
        }

        entity = new TrashDetection
        {
            CameraLatitude = dto.CameraLatitude,
            CameraLongitude = dto.CameraLongitude,
            PhotoTakenAtUtc = dto.PhotoTakenAtUtc,
            TemperatureCelsius = dto.TemperatureCelsius,
            Type = trashType,
            Statiegeld = dto.Statiegeld,
            FetchedAtUtc = DateTime.UtcNow
        };
        error = null;
        return true;
    }


       
}