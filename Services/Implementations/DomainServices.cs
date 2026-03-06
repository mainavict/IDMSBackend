using  IDMSBackend.Data;
using IDMSBackend.Wrappers;
using IDMSBackend.DTOs;
using IDMSBackend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using IDMSBackend.Models;


namespace IDMSBackend.Services.Implementations;

public class DomainServices
{
    private ILogger<RoleServices> _logger;
    private readonly AppDbContext _context;

    public DomainServices(AppDbContext context, ILogger<RoleServices> logger)
    {
        _context = context;
        _logger = logger;
    }

    
    public async Task<ApiResponse<ResponseDomainsDtos>> CreateDomainAsync(string domainName)
    {
        try
        {
                if (string.IsNullOrWhiteSpace(domainName))
                {
                    return ApiResponse<ResponseDomainsDtos>.FailureResponse("Domain name cannot be empty", 400);
                }
    
                domainName = char.ToUpper(domainName.Trim()[0]) + domainName.Trim().Substring(1).ToLower();
            
            var existingDomain = await _context.Domains.FirstOrDefaultAsync(d => d.Name == domainName);
            if (existingDomain != null)
            {
                return ApiResponse<ResponseDomainsDtos>.FailureResponse("Domain already exists", 400);
            }

            var newDomain = new Domains
            {
                Name = domainName
            };  
            _context.Domains.Add(newDomain);
            await _context.SaveChangesAsync();

            var domainDto = new ResponseDomainsDtos { Id = newDomain.Id, Name = newDomain.Name };
            return ApiResponse<ResponseDomainsDtos>.SuccessResponse(domainDto, "Domain created successfully", 201);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating domain");
            return ApiResponse<ResponseDomainsDtos>.FailureResponse("An error occurred while creating the domain", 500);
        }
    }
    
    
    public async Task<ApiResponse<List<ResponseDomainsDtos>>> GetAllDomainsAsync()
    {
        try
        {
            var domains = await _context.Domains.AsNoTracking().ToListAsync();
            var domainDtos = domains.Select(d => new ResponseDomainsDtos { Id = d.Id, Name = d.Name }).ToList();
            return ApiResponse<List<ResponseDomainsDtos>>.SuccessResponse(domainDtos, "Domains retrieved successfully", 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving domains");
            return ApiResponse<List<ResponseDomainsDtos>>.FailureResponse("An error occurred while retrieving domains", 500);
        }
    }
    
}