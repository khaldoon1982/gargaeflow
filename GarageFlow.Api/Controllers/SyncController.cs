using GarageFlow.Api.Services;
using GarageFlow.Application.DTOs.Sync;
using Microsoft.AspNetCore.Mvc;

namespace GarageFlow.Api.Controllers;

[ApiController]
[Route("api/sync")]
public class SyncController : ControllerBase
{
    private readonly SyncProcessor _processor;

    public SyncController(SyncProcessor processor) { _processor = processor; }

    [HttpPost("push")]
    public async Task<ActionResult<PushResponse>> Push([FromBody] PushRequest request)
    {
        if (string.IsNullOrEmpty(request.DeviceId))
            return BadRequest(new { error = "DeviceId is verplicht" });

        var response = await _processor.ProcessPushAsync(request);
        return Ok(response);
    }

    [HttpPost("pull")]
    public async Task<ActionResult<PullResponse>> Pull([FromBody] PullRequest request)
    {
        if (string.IsNullOrEmpty(request.DeviceId))
            return BadRequest(new { error = "DeviceId is verplicht" });

        var response = await _processor.ProcessPullAsync(request);
        return Ok(response);
    }

    [HttpGet("status")]
    public IActionResult Status()
    {
        return Ok(new
        {
            status = "online",
            serverTime = DateTime.UtcNow,
            version = "1.0.0"
        });
    }
}
