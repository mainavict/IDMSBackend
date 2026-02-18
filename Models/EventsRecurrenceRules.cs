using  System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IDMSBackend.Models;

public enum RecurrenceFrequency
{
    Daily,
    Weekly,
    Monthly,
    Yearly
}

public class EventsRecurrenceRules
{
    public Guid Id { get; set; } = Guid.NewGuid();
    [ForeignKey("Event")]
    public Guid EventId { get; set; }
    
    public Events Event { get; set; } = null!;
    
    public RecurrenceFrequency Frequency { get; set; }  // daily / weekly / monthly / yearly
    public int Interval { get; set; } = 1; // every  1 day
    public DayOfWeek DayOfWeek { get; set; } //weekly  recurrences
    public int DayOfMonth { get; set; } // monthly  recurrences
    public int MonthOfYear { get; set; } //yearly   recurrences
    
    
    
    
    
}