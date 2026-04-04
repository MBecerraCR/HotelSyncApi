using Microsoft.AspNetCore.Mvc;
using HotelSyncApi.Services;

namespace HotelSyncApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SyncController : ControllerBase
{
    private readonly SyncEngine _engine;

    public SyncController(SyncEngine engine)
    {
        _engine = engine;
    }

    [HttpPost("run")]
    public async Task<IActionResult> RunSync()
    {
        // Este es el método que creamos en el paso 7.1
        await _engine.RunOperaToHubSpotSyncAsync();
        return Ok("Sync cycle completed.");
    }
}