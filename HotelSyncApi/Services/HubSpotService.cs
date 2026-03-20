using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace HotelSyncApi.Services;

public class HubSpotService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl = "https://api.hubapi.com";

    public HubSpotService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        var token = configuration["HubSpot:AccessToken"];
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    // Crear o actualizar un Contacto
    public async Task<string?> CreateContactAsync(string firstName, string lastName, string email, string phone)
    {
        var payload = new
        {
            properties = new
            {
                firstname = firstName,
                lastname = lastName,
                email = email,
                phone = phone
            }
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"{_baseUrl}/crm/v3/objects/contacts", content);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            using var doc = JsonDocument.Parse(responseBody);
            return doc.RootElement.GetProperty("id").GetString();
        }

        // Si falla, loggear el error
        Console.WriteLine($"HubSpot error: {response.StatusCode} - {responseBody}");
        return null;
    }

    // Crear un Deal
    public async Task<string?> CreateDealAsync(string dealName, string amount, string hotelCode)
    {
        var payload = new
        {
            properties = new Dictionary<string, string>
            {
                ["dealname"] = dealName,
                ["amount"] = amount,
                ["dealstage"] = "closedwon",
                ["pipeline"] = "default"
            }
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"{_baseUrl}/crm/v3/objects/deals", content);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            using var doc = JsonDocument.Parse(responseBody);
            return doc.RootElement.GetProperty("id").GetString();
        }

        Console.WriteLine($"HubSpot error: {response.StatusCode} - {responseBody}");
        return null;
    }

    // Asociar un Contacto a un Deal
    public async Task AssociateContactToDealAsync(string contactId, string dealId)
    {
        var payload = new
        {
            inputs = new[]
            {
                new
                {
                    from = new { id = dealId },
                    to = new { id = contactId },
                    type = "deal_to_contact"
                }
            }
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        await _httpClient.PostAsync($"{_baseUrl}/crm/v4/associations/deals/contacts/batch/create", content);
    }
}