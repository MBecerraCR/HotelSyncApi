using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HotelSyncApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WebhookController : ControllerBase
{
    private readonly ILogger<WebhookController> _logger;

    public WebhookController(ILogger<WebhookController> logger)
    {
        _logger = logger;
    }

    // HubSpot calls this endpoint whenever a subscribed event occurs
    [HttpPost("hubspot")]
    public async Task<IActionResult> ReceiveHubSpotEvent()
    {
        // Reading the request body
        using var reader = new StreamReader(Request.Body);
        var jsonBody = await reader.ReadToEndAsync();

        _logger.LogInformation("Incoming Webhook received from HubSpot: {body}", jsonBody);

        try
        {
            // HubSpot sends an array of events
            var events = JsonSerializer.Deserialize<List<HubSpotWebhookEvent>>(jsonBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            foreach (var hubspotEvent in events ?? Enumerable.Empty<HubSpotWebhookEvent>())
            {
                _logger.LogInformation("Event: {type} | Object ID: {id} | Property: {prop} = {val}",
                    hubspotEvent.SubscriptionType,
                    hubspotEvent.ObjectId,
                    hubspotEvent.PropertyName,
                    hubspotEvent.PropertyValue);

                // Check if a Deal reached 'Closed Won' stage
                if (hubspotEvent.SubscriptionType == "deal.propertyChange" &&
                    hubspotEvent.PropertyName == "dealstage" &&
                    hubspotEvent.PropertyValue == "closedwon")
                {
                    _logger.LogWarning("ALERT: Deal {id} moved to CLOSED WON. Triggering Opera Cloud Sync...", hubspotEvent.ObjectId);

                    // TODO: Implement the reverse sync logic (HubSpot -> Opera) here
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process HubSpot webhook payload.");
            return BadRequest();
        }

        // Always return 200 OK so HubSpot knows we received the data
        return Ok();
    }
}

// Data Transfer Object (DTO) for HubSpot Webhook Payload
public class HubSpotWebhookEvent
{
    public long ObjectId { get; set; }
    public string SubscriptionType { get; set; } = string.Empty;
    public string PropertyName { get; set; } = string.Empty;
    public string PropertyValue { get; set; } = string.Empty;
    public long EventId { get; set; }
    public long OccurredAt { get; set; }
}