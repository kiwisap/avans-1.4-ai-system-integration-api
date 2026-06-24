using avans_1._4_ai_system_integration_api.Models.Dtos;
using avans_1._4_ai_system_integration_api.Services.Interfaces;
using System.Text.Json;

namespace avans_1._4_ai_system_integration_api.Services;

public class SensorApiService(HttpClient http, IConfiguration configuration) : AbstractAuthService(http), ISensorApiService
{
    private static readonly JsonSerializerOptions jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public override string Email => configuration.GetValue<string>("SensorApi:Email") ?? throw new InvalidOperationException("SensorApi:Email is not configured.");

    public override string Password => configuration.GetValue<string>("SensorApi:Password") ?? throw new InvalidOperationException("SensorApi:Password is not configured.");

    public async Task<List<SensorTrashDataDto>> GetDetectionsAsync(DateTime startDate, DateTime endDate)
    {
        var request = new SensorTrashTimeframeDto
        {
            StartTime = startDate,
            EndTime = endDate
        };

        var json = JsonSerializer.Serialize(request);
        var response = await SendPostRequest($"trash/timeframe", json);
        if (response is WebRequestData<string> data)
        {
            return JsonSerializer.Deserialize<List<SensorTrashDataDto>>(data.Data, jsonOptions) ?? [];
        }

        return [];
    }
}