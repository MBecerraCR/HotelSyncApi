namespace HotelSyncWorker;

// Inherit from BackgroundService, the standard .NET base class for long-running tasks
public class OperaPollingWorker : BackgroundService
{
    private readonly ILogger<OperaPollingWorker> _logger;

    // Polling interval set to 10 seconds for testing purposes. 
    // In production, this would typically be 10 minutes.
    private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(10);

    public OperaPollingWorker(ILogger<OperaPollingWorker> logger)
    {
        _logger = logger;
    }

    // This method runs once when the service starts
    // Inside HotelSyncWorker -> Worker.cs
    // Dentro de HotelSyncWorker -> OperaPollingWorker.cs
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("HotelSync Worker started at: {time}", DateTimeOffset.Now);

        // En la vida real, el Worker necesita un HttpClient para "despertar" a la API
        using var httpClient = new HttpClient();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("--- Starting scheduled sync trigger ---");

                // SUSTITUCIÓN DE LA VIDA REAL: 
                // En lugar de hacer la lógica aquí, el Worker le avisa a la API que trabaje.
                // Por ahora, usaremos la URL de tu API local (ajusta el puerto si es necesario)
                var response = await httpClient.PostAsync("https://localhost:7224/api/sync/run", null, stoppingToken);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Sync trigger sent successfully to HotelSyncApi.");
                }
                else
                {
                    _logger.LogError("Failed to trigger sync. API responded with: {status}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while trying to trigger the SyncEngine.");
            }

            // El intervalo de 15 minutos según el SOW
            await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
        }
    }

    private async Task PollOperaCloudAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Connecting to Opera Cloud API...");

        // Mock data representing new reservations found during polling
        var mockReservations = new[]
        {
            new { ConfNo = "CONF-777", Guest = "Mauricio Becerra", Hotel = "SJO_Center" },
            new { ConfNo = "CONF-888", Guest = "Kevin Okamura", Hotel = "SJO_East" }
        };

        foreach (var res in mockReservations)
        {
            // Check if cancellation was requested mid-process
            if (cancellationToken.IsCancellationRequested) break;

            _logger.LogInformation(">> New reservation detected: {0} | Guest: {1}", res.ConfNo, res.Guest);

            // Logic to sync with HubSpot will be implemented here in the next phase
            await Task.Delay(500, cancellationToken);
        }

        _logger.LogInformation("Cycle completed successfully.");
    }


}