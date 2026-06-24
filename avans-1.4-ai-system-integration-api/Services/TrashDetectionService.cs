using avans_1._4_ai_system_integration_api.Extensions;
using avans_1._4_ai_system_integration_api.Mapping.Interfaces;
using avans_1._4_ai_system_integration_api.Models.Dtos;
using avans_1._4_ai_system_integration_api.Models.Entities;
using avans_1._4_ai_system_integration_api.Models.Enums;
using avans_1._4_ai_system_integration_api.Repositories.Interfaces;
using avans_1._4_ai_system_integration_api.Services.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace avans_1._4_ai_system_integration_api.Services;

public class TrashDetectionService(
    ISensorApiService sensorApiService,
    ITrashDetectionMappingService trashDetectionMappingService,
    ITrashDetectionRepository repository,
    ILogger<TrashDetectionService> logger) : ITrashDetectionService
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);

    public async Task<List<TrashDetectionDto>> GetTrashDataAsync(DateTime from, DateTime to)
    {
        if (from > to)
            throw new ValidationException("'from' moet voor 'to' liggen.");
        
        if (to > DateTime.UtcNow)
            throw new ValidationException("'to' mag niet in de toekomst liggen.");

        var fetchLog = await repository.FindFetchLogAsync(from, to);
        var isFresh = fetchLog != null && DateTime.UtcNow - fetchLog.FetchedAtUtc < CacheDuration; ;

        if (isFresh)
        {
            logger.LogInformation("Data voor range {From} - {To} is nog vers, ophalen uit database", from, to);
            var entitiesFresh = await repository.GetByRangeAsync(from, to);
            return [.. entitiesFresh.Select(trashDetectionMappingService.TrashDetectionToTrashDetectionDto)];
        }

        logger.LogInformation("Data voor range {From} - {To} is niet vers, ophalen van API", from, to);

        var sensorData = await sensorApiService.GetDetectionsAsync(from, to);
        var entities = MapAndValidate(sensorData);

        await repository.AddRangeAsync(entities);
        await repository.AddFetchLogAsync(new TrashDataFetchLog
        {
            RangeFrom = from,
            RangeTo = to,
            FetchedAtUtc = DateTime.UtcNow
        });

        return [.. entities.Select(trashDetectionMappingService.TrashDetectionToTrashDetectionDto)];
    }

    private List<TrashDetection> MapAndValidate(List<SensorTrashDataDto> dTOs)
    {
        var result = new List<TrashDetection>();

        foreach (var dto in dTOs)
        {
            if (!TryValidateAndMap(dto, out var entity, out var error))
            {
                logger.LogWarning("Ongeldige data gedetecteerd: {Error}", error);
                continue;
            }
            result.Add(entity);
        }
        return result;
    }

    private bool TryValidateAndMap(SensorTrashDataDto dto, out TrashDetection? entity, out string? error)
    {
        entity = null;
        
        if (!Enum.TryParse<TrashType>(dto.TrashType.ToTitleCaseWithUnderscores(), true, out var trashType))
        {
            error = $"Onbekend afvaltype: {dto.TrashType}";
            return false;
        }
        if (dto.Latitude < -90 || dto.Latitude > 90)
        {
            error = "Latitude buiten geldig bereik (-90 tot 90)";
            return false;
        }

        if (dto.Longitude < -180 || dto.Longitude > 180)
        {
            error = "Longitude buiten geldig bereik (-180 tot 180)";
            return false;
        }
        if (dto.DateTime > DateTime.UtcNow)
        {
            error = "Tijdstip foto ligt in de toekomst";
            return false;
        }

        entity = new TrashDetection
        {
            SensorId = dto.Id,
            TrashType = trashType.ToString(),
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            DateTime = dto.DateTime,
            Temperature = dto.Temperature,
            Rain = dto.Rain,
            Confidence = dto.Confidence,
            ImageId = dto.ImageId,
            FetchedAtUtc = DateTime.UtcNow
        };
        error = null;
        return true;
    }


       
}