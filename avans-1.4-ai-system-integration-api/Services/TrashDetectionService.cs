using avans_1._4_ai_system_integration_api.Models.DTOs;
using avans_1._4_ai_system_integration_api.Models.Entities;
using avans_1._4_ai_system_integration_api.Repositories.Interfaces;
using avans_1._4_ai_system_integration_api.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace avans_1._4_ai_system_integration_api.Services;

public class TrashDetectionService : ITrashDetectionService
{
    private readonly ISensorApiClient _sensorApiClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<TrashDetectionService> _logger;

    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);

    public TrashDetectionService(
        ISensorApiClient sensorApiClient,
        IMemoryCache cache,
        ILogger<TrashDetectionService> logger)
    {
        _sensorApiClient = sensorApiClient;
        _cache = cache;
        _logger = logger;
    }

    public async Task<List<SensorTrashDataDTO>> GetTrashDataAsync(DateTime from, DateTime to)
    {
        var cacheKey = BuildCacheKey(from, to);

        // check of de data al in cache staat en nog geldig is
        if (_cache.TryGetValue(cacheKey, out List<SensorTrashDataDTO>? cachedData))
        {
            _logger.LogInformation("Cache hit voor range {From} - {To}", from, to);
            return cachedData!;
        }

        // niet gecached of verlopen, dus data ophalen van API
        _logger.LogInformation("Cache miss voor range {From} - {To}, data ophalen", from, to);
        var freshData = await _sensorApiClient.GetLatestDetectionsAsync(from, to);

        //opslaan in cache met expiratie
        _cache.Set(cacheKey, freshData, CacheDuration);

        return freshData;
    }

    private static string BuildCacheKey(DateTime from, DateTime to)
        => $"trashdata_{from:yyyyMMddHHmm}_{to:yyyyMMddHHmm}";
}