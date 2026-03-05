using IDMSBackend.DTOs;
using IDMSBackend.Services.Interfaces;

namespace IDMSBackend.BackgroundServices;

public class StudentSyncWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<StudentSyncWorker> _logger;
    private readonly IConfiguration _configuration;

    public StudentSyncWorker(IServiceProvider serviceProvider, ILogger<StudentSyncWorker> logger, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _configuration = configuration;
        
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Student Sync Worker initialized to run daily at 2:00 AM.");

        while (!stoppingToken.IsCancellationRequested)
        {
            int hour = _configuration.GetValue<int>("SyncSettings:RunHour");
            int minute = _configuration.GetValue<int>("SyncSettings:RunMinute");
            var now = DateTime.Now;
            var nextRunTime = DateTime.Today.AddHours(hour).AddMinutes(minute); // 2:00 AM today

            // If it's already past 2:00 AM today, schedule for 2:00 AM tomorrow
            if (now > nextRunTime)
            {
                nextRunTime = nextRunTime.AddDays(1);
            }

            var delay = nextRunTime - now;
            _logger.LogInformation("Next sync scheduled for {Time}. Waiting {Delay} hours.", nextRunTime, Math.Round(delay.TotalHours, 2));

            try
            {
                // 1. Wait until the scheduled time (2:00 AM)
                await Task.Delay(delay, stoppingToken);

                _logger.LogInformation("Scheduled sync starting at {Time}...", DateTime.Now);

                // 2. Create a scope to access Scoped Services (DbContext/StudentService)
                using (var scope = _serviceProvider.CreateScope())
                {
                    var studentService = scope.ServiceProvider.GetRequiredService<IStudentServices>();
                    
                    // 3. Fetch data from external source
                    var externalData = await FetchExternalDataAsync();

                    if (externalData != null && externalData.Any())
                    {
                        // 4. Run the Sync logic
                        var result = await studentService.SyncStudentsAsync(externalData);
                        
                        _logger.LogInformation("Sync Result: Added {Added}, Updated {Updated}. Took {Time}ms", 
                            result.Data.AddedCount, result.Data.UpdatedCount, result.Data.ExecutionTimeMs);
                    }
                    else
                    {
                        _logger.LogWarning("Scheduled sync aborted: No data received from external source.");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Student Sync Worker is stopping (Cancellation requested).");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during the automated 2:00 AM sync.");
            }

            // Small buffer to prevent the loop from immediately calculating "today" again 
            // if the sync finished in less than a second.
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }

    private async Task<List<StudentSyncDto>> FetchExternalDataAsync()
    {
        // This is where you call your University's Database or API
        // For now, I'm returning an empty list. Replace this with your actual fetch logic.
        return new List<StudentSyncDto>();
    }
}