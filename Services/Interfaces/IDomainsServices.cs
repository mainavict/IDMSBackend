using  IDMSBackend.Wrappers;
using IDMSBackend.DTOs;

namespace IDMSBackend.Services.Interfaces;

public interface IDomainsServices
{
    Task <ApiResponse<ResponseDomainsDtos>>CreateDomainAsync(string domainName);
    Task<ApiResponse<List<ResponseDomainsDtos>>> GetAllDomainsAsync();
}