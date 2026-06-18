namespace avans_1._4_ai_system_integration_api.Models.DTOs;
public class SensorTrashDataDTO
{
    public double CameraLatitude { get; set; }
    public double CameraLongitude { get; set; }
    public DateTime PhotoTakenAtUtc { get; set; }
    public double TemperatureCelsius { get; set; }
    public string Type { get; set; } = default!;
    public bool Statiegeld { get; set; }
}