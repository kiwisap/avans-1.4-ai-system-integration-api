namespace avans_1._4_ai_system_integration_api.Models.Entities;

public class TrashDataFetchLog
{
    public int Id { get; set; }
    public DateTime RangeFrom { get; set; }
    public DateTime RangeTo { get; set; }
    public DateTime FetchedAtUtc { get; set; }
}