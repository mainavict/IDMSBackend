namespace IDMSBackend.Models;

public class AuditLog
{
    // 1. The Unique Identity of the log entry itself
    public Guid Id { get; set; } = Guid.NewGuid();

    // 2. The "Who" - Can be a Name or a GUID from your Users table
    public string? UserId { get; set; } 

    // 3. The "What" - A quick name for the event
    public string Action { get; set; } = string.Empty; 

    // 4. The "Details" - A full sentence describing exactly what changed
    public string? Details { get; set; } 

    // 5. The "Where" - The network address
    public string? IpAddress { get; set; }

    // 6. The "Device" - Helps identify if they used a Phone or PC
    public string? UserAgent { get; set; } 

    // 7. The "Tracking ID" - Matches the technical logs in your console
    public string? TraceId { get; set; } 

    // 8. The "When" - Always stored in UTC to avoid time-zone confusion
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}