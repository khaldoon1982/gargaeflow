using GarageFlow.Api.Data;
using GarageFlow.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GarageFlow.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly ApiDbContext _db;

    public AuthController(ApiDbContext db) { _db = db; }

    [HttpPost("device")]
    public async Task<IActionResult> RegisterDevice([FromBody] DeviceRegistrationRequest request)
    {
        if (string.IsNullOrEmpty(request.DeviceId))
            return BadRequest(new { error = "DeviceId is verplicht" });

        var existing = await _db.DeviceRegistrations.FirstOrDefaultAsync(d => d.DeviceId == request.DeviceId);
        if (existing is not null)
        {
            existing.LastSeenAtUtc = DateTime.UtcNow;
            existing.DeviceName = request.DeviceName ?? existing.DeviceName;
        }
        else
        {
            _db.DeviceRegistrations.Add(new DeviceRegistration
            {
                DeviceId = request.DeviceId,
                DeviceName = request.DeviceName
            });
        }
        await _db.SaveChangesAsync();

        return Ok(new { status = "registered", deviceId = request.DeviceId });
    }
}

public class DeviceRegistrationRequest
{
    public string DeviceId { get; set; } = string.Empty;
    public string? DeviceName { get; set; }
}
