using IDMSBackend.Models;


namespace IDMSBackend.DTOs;



public class CreateEventDto
{
    public string Name { get; set; } = string.Empty;
    public EventTypes EventType { get; set; }
    public bool IsCritical { get; set; }
    public DateTime ActiveFrom { get; set; }
    public DateTime ActiveUntil { get; set; }
    public string StartTime { get; set; } // Format: "HH:mm"
    public string EndTime { get; set; }   // Format: "HH:mm"
    public int ScanStartOffset { get; set; } = 15;
    public string Description { get; set; } = string.Empty;
    public Guid DomainId { get; set; }
    
    // Recurrence Fields
    public bool IsRecurring { get; set; }= false;
    public RecurrenceFrequency? Frequency { get; set; }
    public DayOfWeek? DayOfWeek { get; set; } // For Weekly
    public int DayOfMonth { get; set; } // For Monthly
    public int MonthOfYear { get; set; } // For Yearly
}

public class ActiveScanEventDto
{
    public Guid EventId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DomainName { get; set; } = string.Empty;
    public string StartTime { get; set; }
    public string EndTime { get; set; }
    public bool IsCritical { get; set; }
}

public class EventDetailsDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public EventTypes EventType { get; set; }
    public bool IsCritical { get; set; }
    public DateTime ActiveFrom { get; set; }
    public DateTime ActiveUntil { get; set; }
    public string StartTime { get; set; }
    public string EndTime { get; set; }
    public int ScanStartOffset { get; set; }
    public string Description { get; set; } = string.Empty;
    public string DomainName { get; set; } = string.Empty;
    
    // Recurrence Details
    public bool IsRecurring { get; set; }
    public RecurrenceFrequency? Frequency { get; set; }
    public DayOfWeek? DayOfWeek { get; set; } // For Weekly\
    public int DayOfMonth { get; set; } // For Monthly
    public int MonthOfYear { get; set; } // For Yearly
}

public class UpdateEventDto
{
    public string Name { get; set; } = string.Empty;
    public EventTypes EventType { get; set; }
    public bool IsCritical { get; set; }
    public DateTime ActiveFrom { get; set; }
    public DateTime ActiveUntil { get; set; }
    public string StartTime { get; set; } // Format: "HH:mm"
    public string EndTime { get; set; }   // Format: "HH:mm"
    public int ScanStartOffset { get; set; } = 15;
    public string Description { get; set; } = string.Empty;
    
    // Recurrence Fields
    public bool IsRecurring { get; set; }
    public RecurrenceFrequency? Frequency { get; set; }
    public DayOfWeek? DayOfWeek { get; set; } // For Weekly
    public  int DayOfMonth { get; set; } // For Monthly
     public int MonthOfYear { get; set; } // For Yearly
}

public class DeleteEventDto
{
    public Guid EventId { get; set; }
}

