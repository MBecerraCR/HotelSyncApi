using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HotelSyncApi.Controllers;

[ApiController] //sAYS THIS CLASS IS AN API CONTROLLER, ENABLES FEATURES LIKE MODEL BINDING, VALIDATION, ETC.
[Route("api/[controller]")] //sAYS THE BASE URL FOR THIS CONTROLLER IS api/reservations
public class ReservationsController : ControllerBase
{
    // GET api/reservations
    [HttpGet]
    public IActionResult GetAll()
    {
        // Por ahora retornamos datos de prueba
        var reservations = new[]
        {
            new { Id = "RES001", GuestName = "Juan Pérez", Hotel = "Argentina Hotel A", Status = "RESERVED" },
            new { Id = "RES002", GuestName = "María López", Hotel = "Chile Hotel A", Status = "CHECKED_IN" }
        };

        return Ok(reservations);
    }

    // GET api/reservations/RES001
    [HttpGet("{confirmationNumber}")]
    public IActionResult GetById(string confirmationNumber)
    {
        var reservation = new
        {
            Id = confirmationNumber,
            GuestName = "Juan Pérez",
            Hotel = "Argentina Hotel A",
            Status = "RESERVED",
            Arrival = "2026-04-01",
            Departure = "2026-04-05"
        };

        return Ok(reservation);
    }

    // GET api/reservations/email
    [HttpGet("email/{email}")]
    public async Task<IActionResult> GetByEmail(string email)
    {
        // Validate email format
        if (!IsValidEmail(email))
            return BadRequest("Invalid email format");

        // Temporary in-memory lookup until a reservation service is available
        var reservations = new[]
        {
            new { Id = "RES001", GuestName = "Juan Pérez", Email = "juan.perez@example.com", Hotel = "Argentina Hotel A", Status = "RESERVED" },
            new { Id = "RES002", GuestName = "María López", Email = "maria.lopez@example.com", Hotel = "Chile Hotel A", Status = " CHECKED_IN" }
        };

        var reservation = reservations.FirstOrDefault(r => string.Equals(r.Email, email, StringComparison.OrdinalIgnoreCase));

        if (reservation == null)
            return NotFound($"No reservation found for {email}");

        return await Task.FromResult(Ok(reservation));
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

}