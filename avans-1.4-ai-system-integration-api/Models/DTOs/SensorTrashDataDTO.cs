using avans_1._4_ai_system_integration_api.Models.Enums;

namespace avans_1._4_ai_system_integration_api.Models.DTOs;
public class SensorTrashDataDTO
{
    public Guid Id { get; set; }
    public string TrashType { get; set; } = default!;
    public float Latitude { get; set; }
    public float Longitude { get; set; }
    public DateTime DateTime { get; set; }
    public float Temperature { get; set; }
    public float Rain { get; set; }
    public float Confidence { get; set; }
    public Guid ImageId { get; set; }
}