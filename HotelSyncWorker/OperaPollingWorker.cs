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
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("--- Opera Polling Worker Started ---");

        // Main loop: continues running until the service is stopped
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Executing polling cycle at: {time}", DateTimeOffset.Now);
                await PollOperaCloudAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                // If a cycle fails, we log the error but keep the worker alive for the next attempt
                _logger.LogError(ex, "An unexpected error occurred during the polling cycle.");
            }

            _logger.LogInformation("Waiting {s} seconds for the next cycle...", _pollingInterval.TotalSeconds);

            // Task.Delay is asynchronous and doesn't block the main execution thread
            await Task.Delay(_pollingInterval, stoppingToken);
        }

        _logger.LogWarning("Worker is shutting down (Cancellation requested).");
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