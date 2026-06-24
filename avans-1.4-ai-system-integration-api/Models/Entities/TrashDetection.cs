using avans_1._4_ai_system_integration_api.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace avans_1._4_ai_system_integration_api.Models.Entities;

public class TrashDetection
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid SensorId { get; set; }
    public string TrashType { get; set; }
    public float Latitude { get; set; }
    public float Longitude { get; set; }
    public DateTime DateTime { get; set; }
    public float Temperature { get; set; }
    public float Rain { get; set; }
    public float Confidence { get; set; }
    public Guid ImageId { get; set; }
    public DateTime FetchedAtUtc { get; set; }
}