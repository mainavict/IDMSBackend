using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace IDMSBackend.Models;
public enum EventTypes
{
    Cafeteria,
    Residence,
    Church,
    Assembly,
    Exams
    
}
public class Events
{
    [Key]
    public Guid Id { get; set; }= Guid.NewGuid();
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    public EventTypes EventType { get; set; }
    
    public bool IsCritical { get; set; } = true;
    
    public DateTime ActiveFrom { get; set; } // August 2
    
    public DateTime ActiveUntil { get; set; } // May 1
    
    public  TimeOnly StartTime { get; set; }  // 08:00
    
    public TimeOnly EndTime { get; set; } // 17:00
    
    public bool IsRecurring { get; set; } = false;
    
    public int  ScanStartOffset { get; set; } = 15; // Minutes before event start when scanning can begin
    
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;
    
    [ForeignKey("Creator")]     
    public Guid CreatedBy { get; set; } 
    
    public Guid DomainId { get; set; }
    public Domains Domain { get; set; } = null!;
    
    public Guid  UpdatedBy { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; }= DateTime.UtcNow;
    
    public User Creator { get; set; } = null!;
    public User Updater { get; set; } = null!;
    
    public EventsRecurrenceRules? EventsRecurrenceRule { get; set; }
    
}