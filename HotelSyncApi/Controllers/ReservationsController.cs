using HotelSyncApi.Data;
using HotelSyncApi.Services;
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
    
    
    [HttpPost("create-in-hubspot")]
    public async Task<IActionResult> CreateInHubSpot([FromServices] HubSpotService hubspot)
    {
        // Simula que llegó una reserva de Opera y la creamos en HubSpot
        var contactId = await hubspot.CreateContactAsync(
            "María", "García", "maria.garcia@test.com", "+56912345678");

        var dealId = await hubspot.CreateDealAsync(
            "Reserva - María García - Chile Hotel A", "2000", "CHL_A");

        if (contactId != null && dealId != null)
        {
            await hubspot.AssociateContactToDealAsync(contactId, dealId);
        }

        return Ok(new { ContactId = contactId, DealId = dealId });
    }




}

public record TestSyncRequest(string OperaResId, string HotelCode);

