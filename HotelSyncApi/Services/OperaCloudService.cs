using System.Text;
using System.Text.Json;

namespace HotelSyncApi.Services;

public class OperaCloudService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public OperaCloudService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        // Uses the Mock URL in development, would use real Opera URL in production
        _baseUrl = configuration["OperaCloud:BaseUrl"] ?? "https://localhost:7002/api/opera";
    }

    public async Task<List<OperaReservation>> GetReservationsAsync(DateTime from, string hotelCode)
    {
        var url = $"{_baseUrl}/reservations?from={from:O}&hotelCode={hotelCode}";
        var response = await _httpClient.GetAsync(url);
        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Opera Cloud error: {response.StatusCode} - {body}");
        }

        // Deserialize the nested reservations property
        using var doc = JsonDocument.Parse(body);
        var reservationsJson = doc.RootElement.GetProperty("reservations").GetRawText();

        return JsonSerializer.Deserialize<List<OperaReservation>>(reservationsJson,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];
    }

    public async Task<bool> CheckAvailabilityAsync(string hotelCode, DateTime arrival, DateTime departure, string roomType)
    {
        var url = $"{_baseUrl}/availability?hotelCode={hotelCode}&arrival={arrival:O}&departure={departure:O}&roomType={roomType}";
        var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode) return false;

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        return doc.RootElement.GetProperty("available").GetBoolean();
    }

    public async Task<string?> CreateReservationAsync(CreateOperaReservationDto dto)
    {
        var json = JsonSerializer.Serialize(dto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"{_baseUrl}/reservations", content);
        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Error creating reservation in Opera Mock: {body}");
            return null;
        }

        using var doc = JsonDocument.Parse(body);
        return doc.RootElement.GetProperty("confirmationNo").GetString();
    }
}

// Data Transfer Objects (DTOs) for the service

public class OperaReservation
{
    public string ConfirmationNo { get; set; } = "";
    public string ResvStatus { get; set; } = "";
    public string Resort { get; set; } = "";
    public OperaGuest Guest { get; set; } = new();
    public string Arrival { get; set; } = "";
    public string Departure { get; set; } = "";
    public int Nights { get; set; }
    public int Adults { get; set; }
    public string OriginOfBooking { get; set; } = "";
    public string MarketCode { get; set; } = "";
    public double ShareAmount { get; set; }
}

public class OperaGuest
{
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
}

public class CreateOperaReservationDto
{
    public string HotelCode { get; set; } = "";
    public string GuestFirstName { get; set; } = "";
    public string GuestLastName { get; set; } = "";
    public string GuestEmail { get; set; } = "";
    public DateTime Arrival { get; set; }
    public DateTime Departure { get; set; }
    public int Adults { get; set; }
    public string RoomType { get; set; } = "";
    public string HubSpotDealId { get; set; } = "";
}