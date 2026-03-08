using IDMSBackend.DTOs;
using IDMSBackend.Models;
using IDMSBackend.Wrappers;

namespace IDMSBackend.Services.Interfaces;

public interface IEventServices
{
    Task<ApiResponse<Guid>> CreateEventAsync(CreateEventDto dto, Guid creatorId);
    Task<ApiResponse<List<ActiveScanEventDto>>> GetCurrentEventsForScannerAsync(Guid userId);
    Task<ApiResponse<List<EventDetailsDto>>> GetAllEventsForDomainAsync(Guid domainId);
    Task<ApiResponse<bool>> UpdateEventAsync(Guid eventId, UpdateEventDto dto, Guid updaterId);
    Task<ApiResponse<bool>> DeleteEventAsync(Guid eventId);
    Task <ApiResponse<List<EventDetailsDto>>> GetActiveEventsForDomainAsync(Guid domainId);
    Task<ApiResponse<List<EventDetailsDto>>> GetAllEvents(Guid domainId);
}