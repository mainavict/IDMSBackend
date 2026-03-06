using IDMSBackend.Models;
using IDMSBackend.Services.Interfaces;
using IDMSBackend.Wrappers;
using IDMSBackend.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace IDMSBackend.Controllers;
[ApiController]
[Route("api/[controller]")]
public class DomainController: ControllerBase
{
    private readonly IDomainsServices _domainService;

    public DomainController(IDomainsServices domainService)
    {
        _domainService = domainService;
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateDomain( string domainName)
    {
        try
        {
            var response = await _domainService.CreateDomainAsync(domainName);
            return StatusCode(response.StatusCode, response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Success = false, Message = "An error occurred while creating the domain.", Details = ex.Message });
        }
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAllDomains()
    {
        try
        {
            var response = await _domainService.GetAllDomainsAsync();
            return StatusCode(response.StatusCode, response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Success = false, Message = "An error occurred while retrieving domains.", Details = ex.Message });
        }
    }
    
    
}