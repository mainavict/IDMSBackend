using IDMSBackend.DTOs;
using IDMSBackend.Models;
using IDMSBackend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IDMSBackend.Controllers;
[ApiController]
[Route("api/[controller]")]
public class EventController: ControllerBase
{
    private readonly IEventServices _eventServices;

    public EventController(IEventServices eventServices)
    {
        _eventServices = eventServices;
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateEvent([FromBody] CreateEventDto dto, [FromHeader] Guid creatorId)
    {
        var result = await _eventServices.CreateEventAsync(dto, creatorId);
        if (result.Success)
            return Ok(result);
        return BadRequest(result);
    }
    
    [HttpGet("scanner/{userId}")]
    public async Task<IActionResult> GetCurrentEventsForScanner(Guid userId)
    {
        var result = await _eventServices.GetCurrentEventsForScannerAsync(userId);
        if (result.Success)
            return Ok(result);
        return BadRequest(result);
    }
    
    [HttpGet("domain/{domainId}")]
    public async Task<IActionResult> GetAllEventsForDomain(Guid domainId)
    {
        var result = await _eventServices.GetAllEventsForDomainAsync(domainId);
        if (result.Success)
            return Ok(result);
        return BadRequest(result);
    }
    
    [HttpPut("{eventId}")]
    public async Task<IActionResult> UpdateEvent(Guid eventId, [FromBody] UpdateEventDto dto, [FromHeader] Guid updaterId)
    {
        var result = await _eventServices.UpdateEventAsync(eventId, dto, updaterId);
        if (result.Success)
            return Ok(result);
        return BadRequest(result);
    }
    
    [HttpDelete("{eventId}")]
    public async Task<IActionResult> DeleteEvent(Guid eventId)
    {
        var result = await _eventServices.DeleteEventAsync(eventId);
        if (result.Success)
            return Ok(result);
        return BadRequest(result);
    }

    [HttpGet("{domainId}")]
    public async Task<IActionResult> GetActiveEventsForDomainAsync(Guid domainId)
    {
        var result = await _eventServices.GetActiveEventsForDomainAsync(domainId);
        if (result.Success)
            return Ok(result);
        return BadRequest(result);
        
    }
    
}