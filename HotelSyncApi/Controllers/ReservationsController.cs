using HotelSyncApi.Data;
using Microsoft.AspNetCore.Mvc;

namespace HotelSyncApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReservationsController : ControllerBase
{
    private readonly SyncRepository _repo;

    public ReservationsController(SyncRepository repo)
    {
        _repo = repo;
    }

    [HttpPost("test-sync")]
    public async Task<IActionResult> TestSync([FromBody] TestSyncRequest request)
    {
        var id = await _repo.CreateSyncRecordAsync(request.OperaResId, request.HotelCode);
        await _repo.UpdateSyncStatusAsync(id, "SUCCESS");

        return Ok(new { Message = "Sync record created", Id = id });
    }
}

public record TestSyncRequest(string OperaResId, string HotelCode);