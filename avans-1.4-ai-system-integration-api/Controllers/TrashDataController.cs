using avans_1._4_ai_system_integration_api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace avans_1._4_ai_system_integration_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TrashDataController : ControllerBase
{
    private readonly ITrashDetectionService _trashDetectionService;

    public TrashDataController(ITrashDetectionService trashDetectionService)
    {
        _trashDetectionService = trashDetectionService;
    }

    // GET /api/trashdata?from=jjjj-mm-dd&to=jjjj-mm-dd
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetTrashData([FromQuery] DateTime from, [FromQuery] DateTime to)
    {
        var data = await _trashDetectionService.GetTrashDataAsync(from, to);
        return Ok(data);
    }
}