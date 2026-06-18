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

    public async Task<List<SensorTrashDataDTO>> GetLatestDetectionsAsync(DateTime from, DateTime to)
    {
        // Pad en query parameters nog aanpassen aan de echte sensoring-API
        var url = $"https://avansict2244909.azurewebsites.net/data?from={from:yyyy-MM-ddTHH:mm:ssZ}&to={to:yyyy-MM-ddTHH:mm:ssZ}";

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var data = await response.Content.ReadFromJsonAsync<List<SensorTrashDataDTO>>();
        return data ?? new List<SensorTrashDataDTO>();
    }
}