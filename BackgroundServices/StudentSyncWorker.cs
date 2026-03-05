using IDMSBackend.DTOs;
using IDMSBackend.Services.Interfaces;

namespace IDMSBackend.BackgroundServices;

public class StudentSyncWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<StudentSyncWorker> _logger;
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory; // Added this

    public StudentSyncWorker(
        IServiceProvider serviceProvider, 
        ILogger<StudentSyncWorker> logger, 
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory) // Added this
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Student Sync Worker initialized.");

        while (!stoppingToken.IsCancellationRequested)
        {
            // Read config inside the loop so changes to appsettings.json can take effect without a restart
            int hour = _configuration.GetValue<int>("SyncSettings:RunHour", 2);
            int minute = _configuration.GetValue<int>("SyncSettings:RunMinute", 30);
            
            var now = DateTime.Now;
            var nextRunTime = DateTime.Today.AddHours(hour).AddMinutes(minute);

            if (now > nextRunTime)
            {
                nextRunTime = nextRunTime.AddDays(1);
            }

            var delay = nextRunTime - now;
            _logger.LogInformation("Next sync scheduled for {Time}. Waiting {Delay} hours.", nextRunTime, Math.Round(delay.TotalHours, 2));

            try
            {
                await Task.Delay(delay, stoppingToken);

                _logger.LogInformation("Scheduled sync starting at {Time}...", DateTime.Now);

                using (var scope = _serviceProvider.CreateScope())
                {
                    var studentService = scope.ServiceProvider.GetRequiredService<IStudentServices>();
                    
                    var externalData = await FetchExternalDataAsync();

                    if (externalData != null && externalData.Any())
                    {
                        var result = await studentService.SyncStudentsAsync(externalData);
                        _logger.LogInformation("Sync Result: Added {Added}, Updated {Updated}.", 
                            result.Data.AddedCount, result.Data.UpdatedCount);
                    }
                }
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                _logger.LogError(ex, "An error occurred during the automated sync.");
            }

            // Buffer to prevent multi-triggers if the sync is faster than 1 second
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }

    private async Task<List<StudentSyncDto>> FetchExternalDataAsync()
    {
        try
        {
            // This uses the client named "UniversityApi" configured in Program.cs
            var client = _httpClientFactory.CreateClient("UniversityApi");
            
            // Adjust the sub-path if necessary (e.g., "api/students")
            var response = await client.GetAsync("/students"); 

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<StudentSyncDto>>() 
                       ?? new List<StudentSyncDto>();
            }
            
            _logger.LogError("External API failed with status code: {Code}", response.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching data from the external endpoint.");
        }

        return new List<StudentSyncDto>();
    }
}