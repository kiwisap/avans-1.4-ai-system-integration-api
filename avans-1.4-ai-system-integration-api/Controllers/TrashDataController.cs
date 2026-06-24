using avans_1._4_ai_system_integration_api.Models.Dtos;
using avans_1._4_ai_system_integration_api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace avans_1._4_ai_system_integration_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TrashDataController(ITrashDetectionService trashDetectionService) : ControllerBase
{
    [HttpPost]
    [Authorize(AuthenticationSchemes = "Identity.Bearer")]
    public async Task<IActionResult> GetTrashData(TrashDataTimeFrameDto trashDataTimeFrame)
    {
        var data = await trashDetectionService.GetTrashDataAsync(trashDataTimeFrame.StartDate, trashDataTimeFrame.EndDate);
        return Ok(data);
    }
}