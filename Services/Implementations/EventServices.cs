using IDMSBackend.Data;
using IDMSBackend.Services.Interfaces;
using IDMSBackend.DTOs;
using IDMSBackend.Models;
using IDMSBackend.Wrappers;
using Microsoft.EntityFrameworkCore;

namespace IDMSBackend.Services.Implementations;

public class EventServices: IEventServices
{
    private ILogger<EventServices> _logger;
    private readonly AppDbContext _context;

    public EventServices(AppDbContext context, ILogger<EventServices> logger)
    {
        _logger = logger;
        _context = context;
    }

    public async  Task<ApiResponse<Guid>> CreateEventAsync(CreateEventDto dto, Guid creatorId)
    {
        var user =await _context.Users.FindAsync(creatorId);
        if (user == null)        {
            _logger.LogWarning("Attempted to create an event with a non-existent creator ID: {CreatorId}", creatorId);
            return ApiResponse<Guid>.SuccessResponse(Guid.Empty, "Creator not found.", 404);
        }
        
        if (user.Status != UserStatus.Active)
        {
            _logger.LogWarning("Attempted to create an event with an inactive creator ID: {CreatorId}", creatorId);
            return ApiResponse<Guid>.SuccessResponse(Guid.Empty, "Creator is not active.", 403);
        }
        
        if (dto.ActiveFrom >= dto.ActiveUntil)
        {
            _logger.LogWarning("Attempted to create an event with invalid date range: ActiveFrom {ActiveFrom} is not before ActiveUntil {ActiveUntil}", dto.ActiveFrom, dto.ActiveUntil);
            return ApiResponse<Guid>.SuccessResponse(Guid.Empty, "ActiveFrom must be before ActiveUntil.", 400);
        }
        
        var newEvent = new Events
        {
            Name = dto.Name,
            EventType = dto.EventType,
            IsCritical = dto.IsCritical,
            ActiveFrom = dto.ActiveFrom.ToUniversalTime(),
            ActiveUntil = dto.ActiveUntil.ToUniversalTime(),
            StartTime = TimeOnly.Parse(dto.StartTime),
            EndTime = TimeOnly.Parse(dto.EndTime),
            ScanStartOffset = dto.ScanStartOffset,
            Description = dto.Description,
            CreatedBy = creatorId,
            DomainId = dto.DomainId,
            IsRecurring = dto.IsRecurring,
            UpdatedBy = creatorId,
            UpdatedAt = DateTime.UtcNow
           
        };  
       
        
        _logger.LogInformation("Created new event: {EventName} with ID: {EventId}", newEvent.Name, newEvent.Id);
        
        
        
        if (dto.IsRecurring)
        {
            newEvent.EventsRecurrenceRule = new EventsRecurrenceRules
            {
                Frequency = dto.Frequency.Value,
                DayOfWeek = dto.DayOfWeek.HasValue ? dto.DayOfWeek.Value : null,
                DayOfMonth = dto.DayOfMonth,
                MonthOfYear = dto.MonthOfYear
            };
        }
        
        _context.Events.Add(newEvent);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Created new event with recurrence: {EventName} with ID: {EventId}", newEvent.Name, newEvent.Id);
        return ApiResponse<Guid>.SuccessResponse(newEvent.Id, "Event created successfully.", 201); 
    }

    public async Task<ApiResponse<List<ActiveScanEventDto>>> GetCurrentEventsForScannerAsync(Guid userId)
    {
        var user = await  _context.Users.FindAsync(userId);
        if (user == null)        {
            _logger.LogWarning("Attempted to retrieve current events for a non-existent user ID {UserId}", userId);
            return ApiResponse<List<ActiveScanEventDto>>.SuccessResponse(new List<ActiveScanEventDto>(), "User not found.", 404);
        }
        
        if (user.Status != UserStatus.Active)
        {
            _logger.LogWarning("Attempted to retrieve current events for an inactive user ID {UserId}", userId);
            return ApiResponse<List<ActiveScanEventDto>>.SuccessResponse(new List<ActiveScanEventDto>(), "User is not active.", 403);
        }
        
        var authorizedDomainIds = await _context.UserDomainRoles
            .AsNoTracking()
            .Where(udr => udr.UserId == userId && udr.Role.Name == "Scanner")
            .Select(udr => udr.DomainId)
            .ToListAsync();
        if (!authorizedDomainIds.Any())        {
            _logger.LogInformation("User ID {UserId} has no scanner roles in any domains", userId);
            return ApiResponse<List<ActiveScanEventDto>>.SuccessResponse(new List<ActiveScanEventDto>(), "No authorized domains found for user.", 200);
        }
        var currentUtc = DateTime.UtcNow;
        var today = currentUtc.Date;
        var currentTime = TimeOnly.FromDateTime(currentUtc);
        
        var  scannerDomainEvents = await _context.Events
            .AsNoTracking().Where(e => authorizedDomainIds.Contains(e.DomainId) &&
                                       e.ActiveFrom.Date <= today && e.ActiveUntil.Date >= today &&
                                       e.StartTime <= currentTime && e.EndTime >= currentTime)
            .Include(e => e.Domain)
            .ToListAsync();
        
        var eventDtos = scannerDomainEvents.Select(e => new ActiveScanEventDto
        {
            EventId = e.Id,
            Name = e.Name,
            DomainName = e.Domain.Name,
            StartTime = e.StartTime.ToString("HH:mm"),
            EndTime = e.EndTime.ToString("HH:mm"),
            IsCritical = e.IsCritical
        }).ToList();
        
        _logger.LogInformation("Retrieved {EventCount} active events for user ID {UserId}", eventDtos.Count, userId);
        return ApiResponse<List<ActiveScanEventDto>>.SuccessResponse(eventDtos, "Active events retrieved successfully.", 200);
        
    }

    public async  Task<ApiResponse<List<EventDetailsDto>>> GetAllEventsForDomainAsync(Guid domainId)
    {
        var domain =await _context.Domains.FindAsync(domainId);
        if (domain == null)        {
            _logger.LogWarning("Attempted to retrieve events for a non-existent domain ID {DomainId}", domainId);
            return ApiResponse<List<EventDetailsDto>>.SuccessResponse(new List<EventDetailsDto>(), "Domain not found.", 404);
        }
        
        var events =await _context.Events
            .AsNoTracking()
            .Where(e => e.DomainId == domainId)
            .Include(e => e.Domain)
            .ToListAsync();
        
        var eventDtos = events.Select(e => new EventDetailsDto
        {
            Id = e.Id,
            Name = e.Name,
            EventType = e.EventType,
            IsCritical = e.IsCritical,
            ActiveFrom = e.ActiveFrom,
            ActiveUntil = e.ActiveUntil,
            StartTime = e.StartTime.ToString("HH:mm"),
            EndTime = e.EndTime.ToString("HH:mm"),
            ScanStartOffset = e.ScanStartOffset,
            Description = e.Description,
            DomainName = e.Domain.Name,
            IsRecurring = e.IsRecurring,
            Frequency = e.EventsRecurrenceRule != null ? e.EventsRecurrenceRule.Frequency : (RecurrenceFrequency?) null,
            DayOfWeek = e.EventsRecurrenceRule != null ? e.EventsRecurrenceRule.DayOfWeek : (DayOfWeek?) null,
            DayOfMonth = e.EventsRecurrenceRule != null ? e.EventsRecurrenceRule.DayOfMonth : 0,
            MonthOfYear = e.EventsRecurrenceRule != null ? e.EventsRecurrenceRule.MonthOfYear : 0
        }).ToList();    
        
        _logger.LogInformation("Retrieved {EventCount} events for domain ID {DomainId}", eventDtos.Count, domainId);
        return ApiResponse<List<EventDetailsDto>>.SuccessResponse(eventDtos, "Events retrieved successfully.", 200);
    }

   public async Task<ApiResponse<bool>> UpdateEventAsync(Guid eventId, UpdateEventDto dto, Guid updaterId)
{
    //Fetch  event with its related recurrence rules
    var targetEvent = await _context.Events
        .Include(e => e.EventsRecurrenceRule)
        .FirstOrDefaultAsync(e => e.Id == eventId);

    if (targetEvent == null)
    {
        _logger.LogWarning("Update failed: Event {EventId} not found.", eventId);
        return ApiResponse<bool>.SuccessResponse(false, "Event not found.", 404);
    }

    //Validate  Updater
    var updater = await _context.Users.FindAsync(updaterId);
    if (updater == null || updater.Status != UserStatus.Active)
    {
        _logger.LogWarning("Update failed: Unauthorized or inactive updater {UpdaterId}", updaterId);
        return ApiResponse<bool>.SuccessResponse(false, "Invalid or inactive updater.", 403);
    }

    //Basic  Field Updates
    if (!string.IsNullOrWhiteSpace(dto.Name)) targetEvent.Name = dto.Name;
    if (!string.IsNullOrWhiteSpace(dto.Description)) targetEvent.Description = dto.Description;
    
    targetEvent.EventType = dto.EventType;
    targetEvent.IsCritical = dto.IsCritical;
    targetEvent.ScanStartOffset = dto.ScanStartOffset;

    //Date/Time  Validation
    if (dto.ActiveFrom != default && dto.ActiveUntil != default)
    {
        if (dto.ActiveFrom >= dto.ActiveUntil)
        {
            return ApiResponse<bool>.SuccessResponse(false, "ActiveFrom must be before ActiveUntil.", 400);
        }
        targetEvent.ActiveFrom = dto.ActiveFrom.ToUniversalTime();
        targetEvent.ActiveUntil = dto.ActiveUntil.ToUniversalTime();
    }

    //Time Parsing
    if (TimeOnly.TryParse(dto.StartTime, out var startTime)) targetEvent.StartTime = startTime;
    if (TimeOnly.TryParse(dto.EndTime, out var endTime)) targetEvent.EndTime = endTime;

    //Recurrence Logic
    if (dto.IsRecurring)
    {
        // Ensure required values exist before accessing .Value
        if (!dto.Frequency.HasValue )
        {
            return ApiResponse<bool>.SuccessResponse(false, "Frequency and DayOfWeek are required for recurring events.", 400);
        }

        if (targetEvent.EventsRecurrenceRule == null)
        {
            targetEvent.EventsRecurrenceRule = new EventsRecurrenceRules();
        }

        targetEvent.EventsRecurrenceRule.Frequency = dto.Frequency.Value;
        targetEvent.EventsRecurrenceRule.DayOfWeek = dto.DayOfWeek.HasValue? dto.DayOfWeek.Value : null;
        targetEvent.EventsRecurrenceRule.DayOfMonth = dto.DayOfMonth;
        targetEvent.EventsRecurrenceRule.MonthOfYear = dto.MonthOfYear;
    }
    else if (targetEvent.EventsRecurrenceRule != null)
    {
        // If it's no longer recurring, remove the rule record
        _context.EventsRecurrenceRules.Remove(targetEvent.EventsRecurrenceRule);
    }

    // 6. Metadata & Persistence
    targetEvent.UpdatedBy = updaterId;
    targetEvent.UpdatedAt = DateTime.UtcNow;

    try 
    {
        await _context.SaveChangesAsync();
        _logger.LogInformation("Updated event {EventId} successfully", eventId);
        return ApiResponse<bool>.SuccessResponse(true, "Event updated successfully.", 200);
    }
    catch (DbUpdateException ex)
    {
        _logger.LogError(ex, "Database error updating event {EventId}", eventId);
        return ApiResponse<bool>.SuccessResponse(false, "Internal server error during update.", 500);
    }
}

    public async Task<ApiResponse<bool>> DeleteEventAsync(Guid eventId)
    {
       var targetEvent = await _context.Events.FindAsync(eventId);
         if (targetEvent == null)
         {
              _logger.LogWarning("Delete failed: Event {EventId} not found.", eventId);
              return ApiResponse<bool>.SuccessResponse(false, "Event not found.", 404);
         }
            _context.Events.Remove(targetEvent);
        try
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation("Deleted event {EventId} successfully", eventId);
            return ApiResponse<bool>.SuccessResponse(true, "Event deleted successfully.", 200);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error deleting event {EventId}", eventId);
            return ApiResponse<bool>.SuccessResponse(false, "Internal server error during deletion.", 500);
        }
    }

    public async  Task<ApiResponse<List<EventDetailsDto>>> GetActiveEventsForDomainAsync(Guid domainId)
    {
        var domain= await _context.Domains.FindAsync(domainId);
        if (domain == null)        {
            _logger.LogWarning("Attempted to retrieve active events for a non-existent domain ID {DomainId}", domainId);
            return ApiResponse<List<EventDetailsDto>>.SuccessResponse(new List<EventDetailsDto>(), "Domain not found.", 404);
        }
        var localNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, 
            TimeZoneInfo.FindSystemTimeZoneById("E. Africa Standard Time"));
        var currentTime = TimeOnly.FromDateTime(localNow);

        var activeEvents = await _context.Events
            .AsNoTracking()
            .Include(e => e.Domain)
            .Where(e => e.DomainId == domainId &&
                        e.ActiveFrom <= DateTime.UtcNow && e.ActiveUntil >= DateTime.UtcNow &&
                        e.StartTime <= currentTime && e.EndTime >= currentTime)
            .ToListAsync();
        
        var eventDtos = activeEvents.Select(e => new EventDetailsDto
        {
            Id = e.Id,
            Name = e.Name,
            EventType = e.EventType, 
            IsCritical = e.IsCritical,
            ActiveFrom = e.ActiveFrom,
            ActiveUntil = e.ActiveUntil,
            StartTime = e.StartTime.ToString("HH:mm"),
            EndTime = e.EndTime.ToString("HH:mm"),
            ScanStartOffset = e.ScanStartOffset,
            Description = e.Description,
            DomainName = e.Domain.Name,
            IsRecurring = e.IsRecurring,
            Frequency = e.EventsRecurrenceRule != null ? e.EventsRecurrenceRule.Frequency : (RecurrenceFrequency?) null,
            DayOfWeek = e.EventsRecurrenceRule != null ? e.EventsRecurrenceRule.DayOfWeek : (DayOfWeek?) null,
            DayOfMonth = e.EventsRecurrenceRule != null ? e.EventsRecurrenceRule.DayOfMonth : 0,
            MonthOfYear = e.EventsRecurrenceRule != null ? e.EventsRecurrenceRule.MonthOfYear : 0
        }).ToList();    
        
        _logger.LogInformation("Retrieved {EventCount} active events for domain ID {DomainId}", eventDtos.Count, domainId);
        return ApiResponse<List<EventDetailsDto>>.SuccessResponse(eventDtos, "Active events retrieved successfully.", 200);
    }

    public Task<ApiResponse<List<EventDetailsDto>>> GetAllEvents(Guid domainId)
    {
        throw new NotImplementedException();
    }
    
}