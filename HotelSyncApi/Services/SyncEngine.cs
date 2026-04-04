using HotelSyncApi.Data;

namespace HotelSyncApi.Services;

public class SyncEngine
{
    private readonly OperaCloudService _opera;
    private readonly HubSpotService _hubspot;
    private readonly SyncRepository _repo; // Using the Interface for best practices
    private readonly ILogger<SyncEngine> _logger;

    // List of hotel codes to synchronize as defined in the SOW
    private readonly string[] _hotelCodes = { "ARG_A", "ARG_B", "CHL_A", "CHL_B", "BRA_A" };

    public SyncEngine(
        OperaCloudService opera,
        HubSpotService hubspot,
        SyncRepository repo,
        ILogger<SyncEngine> logger)
    {
        _opera = opera;
        _hubspot = hubspot;
        _repo = repo;
        _logger = logger;
    }

    // Main Orchestrator: Opera → HubSpot
    public async Task RunOperaToHubSpotSyncAsync()
    {
        _logger.LogInformation("--- Starting Synchronization Cycle: Opera to HubSpot ---");

        foreach (var hotelCode in _hotelCodes)
        {
            try
            {
                await SyncHotelReservationsAsync(hotelCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to sync hotel: {hotel}", hotelCode);
            }
        }

        _logger.LogInformation("--- Synchronization Cycle Completed ---");
    }

    private async Task SyncHotelReservationsAsync(string hotelCode)
    {
        _logger.LogInformation("Processing Hotel: {hotel}", hotelCode);

        // Polling window: look for reservations from the last 15 minutes
        var fetchFrom = DateTime.UtcNow.AddMinutes(-15);
        var reservations = await _opera.GetReservationsAsync(fetchFrom, hotelCode);

        foreach (var reservation in reservations)
        {
            await ProcessIndividualReservationAsync(reservation);
        }
    }

    private async Task ProcessIndividualReservationAsync(OperaReservation reservation)
    {
        // 1. DEDUPLICATION: Check if this confirmation number already exists in our DB
        var existingRecord = await _repo.GetByConfirmationNumberAsync(reservation.ConfirmationNo);


        if (existingRecord != null)
        {
            _logger.LogInformation("Reservation {confNo} already exists. Skipping creation.",
                reservation.ConfirmationNo);
            // Logic for updates could be added here later
            return;
        }

        // 2. INITIALIZE: Create record in DB with PENDING status
        _logger.LogInformation("New reservation detected: {confNo}. Creating pending record...", reservation.ConfirmationNo);

        // Assuming your repository has a method to create a basic record
        var syncId = await _repo.CreateSyncRecordAsync(reservation.ConfirmationNo, reservation.Resort);

        try
        {
            // 3. HUBSPOT CONTACT: Create or Update guest info
            var contactId = await _hubspot.CreateContactAsync(
                reservation.Guest.FirstName,
                reservation.Guest.LastName,
                reservation.Guest.Email,
                reservation.Guest.Phone);

            if (string.IsNullOrEmpty(contactId))
                throw new Exception("HubSpot Contact creation returned null or empty ID.");

            // 4. ROUTING: Determine pipeline and stage based on SOW Appendix F
            var (pipelineId, stageId) = DeterminePipelineAndStage(reservation.Resort, reservation.OriginOfBooking);

            // 5. HUBSPOT DEAL: Create the deal linked to the reservation
            var dealTitle = $"Resort Reservation - {reservation.Guest.FirstName} {reservation.Guest.LastName}";
            var amount = (reservation.Nights * reservation.ShareAmount).ToString();

            var dealId = await _hubspot.CreateDealAsync(dealTitle, amount, reservation.Resort);

            if (string.IsNullOrEmpty(dealId))
                throw new Exception("HubSpot Deal creation returned null or empty ID.");

            // 6. ASSOCIATION: Link the Contact to the Deal
            await _hubspot.AssociateContactToDealAsync(contactId, dealId);

            // 7. FINALIZE: Update DB with SUCCESS status and IDs
            await _repo.UpdateSyncStatusAsync(syncId, "SUCCESS");
            await _repo.SaveHubSpotIdsAsync(syncId, dealId, contactId);

            _logger.LogInformation("✅ Successfully synced {confNo} to HubSpot. Deal ID: {dealId}",
                reservation.ConfirmationNo, dealId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Synchronization failed for Reservation {confNo}", reservation.ConfirmationNo);
            await _repo.UpdateSyncStatusAsync(syncId, "FAILED", ex.Message);
        }
    }

    private (string pipelineId, string stageId) DeterminePipelineAndStage(string hotelCode, string origin)
    {
        // Business Logic based on booking origin
        return origin.ToUpper() switch
        {
            "DIRECT" or "WALK_IN" => ("default", "closedwon"),
            "TRAVEL_AGENT" => ("travel_agencies", "closedwon"),
            "OTA" => ("ota_pipeline", "closedwon"),
            _ => ("default", "closedwon")
        };
    }
}