using Microsoft.AspNetCore.Mvc;

namespace OperaCloudMock.Controllers;

[ApiController]
[Route("api/opera")]
public class OperaReservationsController : ControllerBase
{
    // Mock GET: Simulates retrieving new or updated reservations from Opera
    // Real Opera URL: GET /rsv/v1/reservations
    [HttpGet("reservations")]
    public IActionResult GetReservations([FromQuery] DateTime? from, [FromQuery] string? hotelCode)
    {
        var reservations = new[]
        {
            new
            {
                confirmationNo = "CONF-2026-001",
                resvNameId = "RES-12345",
                resvStatus = "RESERVED",
                resort = hotelCode ?? "ARG_HOTEL_A",
                guest = new
                {
                    nameId = "GUEST-789",
                    firstName = "Carlos",
                    lastName = "Rodríguez",
                    email = "carlos.rodriguez@email.com",
                    phone = "+541112345678",
                    nationality = "AR"
                },
                arrival = "2026-05-01",
                departure = "2026-05-05",
                nights = 4,
                noOfRooms = 1,
                adults = 2,
                children = 0,
                roomCategory = "STANDARD",
                rateCode = "BAR",
                shareAmount = 250.00,
                originOfBooking = "DIRECT",
                sourceCode = "WALK_IN",
                marketCode = "LEISURE",
                paymentMethod = "CC",
                insertDate = DateTime.UtcNow.AddHours(-2)
            }
        };

        return Ok(new { reservations, totalCount = reservations.Length });
    }

    // Mock POST: Simulates creating a new reservation in Opera
    // Real Opera URL: POST /rsv/v1/reservations
    [HttpPost("reservations")]
    public IActionResult CreateReservation([FromBody] CreateReservationRequest request)
    {
        // Log incoming request to the console for debugging
        Console.WriteLine($"[Mock Opera] Creating reservation for: {request.GuestLastName}");

        // Simulate availability check (Logic would go here in a real scenario)
        var isAvailable = true;

        if (!isAvailable)
        {
            return Conflict(new { message = "No availability for selected dates" });
        }

        var confirmationNo = $"CONF-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}";

        return Ok(new
        {
            confirmationNo,
            resvStatus = "RESERVED",
            message = "Reservation created successfully"
        });
    }

    // Mock GET: Simulates checking room availability
    [HttpGet("availability")]
    public IActionResult CheckAvailability(
        [FromQuery] string hotelCode,
        [FromQuery] DateTime arrival,
        [FromQuery] DateTime departure,
        [FromQuery] string roomType)
    {
        return Ok(new
        {
            available = true,
            hotelCode,
            roomType,
            arrival,
            departure,
            availableRooms = 5
        });
    }
}

public class CreateReservationRequest
{
    public string HotelCode { get; set; } = "";
    public string GuestFirstName { get; set; } = "";
    public string GuestLastName { get; set; } = "";
    public string GuestEmail { get; set; } = "";
    public DateTime Arrival { get; set; }
    public DateTime Departure { get; set; }
    public int Adults { get; set; }
    public string RoomType { get; set; } = "";
    public string HubSpotDealId { get; set; } = ""; // Key field for traceability
}