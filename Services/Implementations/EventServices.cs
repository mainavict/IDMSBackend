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

    public Task<ApiResponse<Guid>> CreateEventAsync(CreateEventDto dto, Guid creatorId)
    {
        var user = _context.Users.Find(creatorId);
        if (user == null)        {
            _logger.LogWarning("Attempted to create an event with a non-existent creator ID: {CreatorId}", creatorId);
            return Task.FromResult(ApiResponse<Guid>.SuccessResponse(Guid.Empty, "Creator not found.", 404));
        }
        
        if (dto.ActiveFrom >= dto.ActiveUntil)
        {
            _logger.LogWarning("Attempted to create an event with invalid date range: ActiveFrom {ActiveFrom} is not before ActiveUntil {ActiveUntil}", dto.ActiveFrom, dto.ActiveUntil);
            return Task.FromResult(ApiResponse<Guid>.SuccessResponse(Guid.Empty, "ActiveFrom must be before ActiveUntil.", 400));
        }
        
        var newEvent = new Events
        {
            Name = dto.Name,
            EventType = dto.EventType,
            IsCritical = dto.IsCritical,
            ActiveFrom = dto.ActiveFrom,
            ActiveUntil = dto.ActiveUntil,
            StartTime = TimeOnly.Parse(dto.StartTime),
            EndTime = TimeOnly.Parse(dto.EndTime),
            ScanStartOffset = dto.ScanStartOffset,
            Description = dto.Description,
            CreatedBy = creatorId,
            DomainId = dto.DomainId,
            IsRecurring = dto.IsRecurring
        };  
        _context.Events.Add(newEvent);
        _context.SaveChanges();
        
        _logger.LogInformation("Created new event: {EventName} with ID: {EventId}", newEvent.Name, newEvent.Id);
        
        
        
        if (dto.IsRecurring)
        {
            newEvent.EventsRecurrenceRule = new EventsRecurrenceRules
            {
                Frequency = dto.Frequency.Value,
                DayOfWeek = dto.DayOfWeek.Value,
                DayOfMonth = dto.DayOfMonth,
                MonthOfYear = dto.MonthOfYear
            };
        }
        
        _context.Events.Add(newEvent);
        _context.SaveChanges();
        
        _logger.LogInformation("Created new event with recurrence: {EventName} with ID: {EventId}", newEvent.Name, newEvent.Id);
        return Task.FromResult(ApiResponse<Guid>.SuccessResponse(newEvent.Id, "Event created successfully.", 201)); 
    }

    public async Task<ApiResponse<List<ActiveScanEventDto>>> GetCurrentEventsForScannerAsync(Guid userId)
    {
        var user = await  _context.Users.FindAsync(userId);
        if (user == null)        {
            _logger.LogWarning("Attempted to retrieve current events for a non-existent user ID {UserId}", userId);
            return ApiResponse<List<ActiveScanEventDto>>.SuccessResponse(new List<ActiveScanEventDto>(), "User not found.", 404);
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
        
    }

    public Task<ApiResponse<List<EventDetailsDto>>> GetAllEventsForDomainAsync(Guid domainId)
    {
        throw new NotImplementedException();
    }

    public Task<ApiResponse<bool>> UpdateEventAsync(Guid eventId, UpdateEventDto dto, Guid updaterId)
    {
        throw new NotImplementedException();
    }

    public Task<ApiResponse<bool>> DeleteEventAsync(Guid eventId)
    {
        throw new NotImplementedException();
    }

    public Task<ApiResponse<List<EventDetailsDto>>> GetActiveEventsForDomainAsync(Guid domainId)
    {
        throw new NotImplementedException();
    }

    public Task<ApiResponse<List<EventDetailsDto>>> GetAllEvents(Guid domainId)
    {
        throw new NotImplementedException();
    }
    
}