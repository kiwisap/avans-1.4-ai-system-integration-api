namespace avans_1._4_ai_system_integration_api.Models.Entities;

public enum TrashType
{
    PLASTIC,
    PAPIER_KARTON,
    GLAS,
    BLIK,
    GROFVUIL,
    ELEKTRISCHE_APPARATEN,
    REST
}

public class TrashDetection
{
    public int Id { get; set; }

    public double CameraLatitude { get; set; }
    public double CameraLongitude { get; set; }

    public double TrashLatitude { get; set; }
    public double TrashLongitude { get; set; }

    public DateTime PhotoTakenAtUtc { get; set; }

    public double TemperatureCelsius { get; set; }

    public TrashType Type { get; set; }

    public bool Statiegeld { get; set; }
}