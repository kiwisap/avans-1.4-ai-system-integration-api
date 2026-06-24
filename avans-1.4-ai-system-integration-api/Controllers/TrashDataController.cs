using avans_1._4_ai_system_integration_api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace avans_1._4_ai_system_integration_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TrashDataController : ControllerBase
{
    private readonly ITrashDetectionService _trashDetectionService;
    private readonly ISensorApiService _sensorApiService;

    public TrashDataController(ITrashDetectionService trashDetectionService, ISensorApiService sensorApiService)
    {
        _trashDetectionService = trashDetectionService;
        _sensorApiService = sensorApiService;
    }

    // GET /api/trashdata?from=jjjj-mm-dd&to=jjjj-mm-dd
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetTrashData([FromQuery] DateTime from, [FromQuery] DateTime to)
    {
        var data = await _sensorApiService.GetDetectionsAsync(from, to); // Call the method to fetch data from the sensor API
        return Ok(data);
        //var data = await _trashDetectionService.GetTrashDataAsync(from, to);
        //return Ok(data);
    }
}