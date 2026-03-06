using IDMSBackend.DTOs;
using IDMSBackend.Wrappers;


namespace IDMSBackend.Services.Interfaces;

public interface IUserDomainRole
{
    Task<ApiResponse<UserDomainRoleDtos>> AssignRoleDomainAsync( AllocateRoleToUserDtos  allocateRoleToUserDtos);
    Task<ApiResponse<List<UserDomainRoleDtos>>> GetUserRolesInDomainAsync(Guid userId);
    Task<ApiResponse<bool>> RemoveRoleFromUserInDomainAsync(RemoveRoleFromUserInDomainDtos removeRoleFromUserInDomainDtos);
    Task <ApiResponse<List<UserDomainRoleDtos>>> GetAllDomainusersByDomainNameAsync(string domainName);
    Task <ApiResponse<List<UserDomainRoleDtos>>> GetAllDomainusersByDomainIdAsync(Guid domainId);
}