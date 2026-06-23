using avans_1._4_ai_system_integration_api.Mapping.Interfaces;
using avans_1._4_ai_system_integration_api.Models.DTOs;
using avans_1._4_ai_system_integration_api.Services.Interfaces;
using System.Net.Http.Json;

namespace avans_1._4_ai_system_integration_api.Services;

public class SensorApiClient : ISensorApiClient
{
    private readonly HttpClient _httpClient;

    public SensorApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<SensorTrashDataDTO>> GetLatestDetectionsAsync(DateTime StartDate, DateTime EndDate)
    {
        // Format DateTime as ISO 8601 strings for URL query parameters
        var start = StartDate.ToString("o");
        var end = EndDate.ToString("o");
        var url = $"https://avansict2244909.azurewebsites.net/trash/timeframe?start={Uri.EscapeDataString(start)}&end={Uri.EscapeDataString(end)}";

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var data = await response.Content.ReadFromJsonAsync<List<SensorTrashDataDTO>>();
        return data ?? new List<SensorTrashDataDTO>();
    }
}