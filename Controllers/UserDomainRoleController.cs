using  IDMSBackend.Services.Implementations;
using IDMSBackend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using IDMSBackend.DTOs;
namespace IDMSBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserDomainRoleController: ControllerBase
{
    private readonly IUserDomainRole _userDomainRoleService;
   

    public UserDomainRoleController(IUserDomainRole userDomainRoleService)
    {
        _userDomainRoleService = userDomainRoleService;
        
    }

    [HttpPost("assign-role-domain")]
    public async Task<IActionResult> AssignRoleDomain(AllocateRoleToUserDtos allocateRoleToUserDtos)
    {
        try
        {
            var response = await _userDomainRoleService.AssignRoleDomainAsync(allocateRoleToUserDtos);
            return StatusCode(response.StatusCode, response);
        }
        catch (Exception ex)
        {
            return StatusCode(500,
                new
                {
                    Success = false, Message = "An error occurred while assigning the role to the user in the domain.",
                    Details = ex.Message
                });
        }




    }
    
    
    [HttpGet]
    public async Task<IActionResult> GetUserRolesAndDomains(Guid userId)
    {
        try
        {
            var response = await _userDomainRoleService.GetUserRolesAndDomainsAsync(userId);
            return StatusCode(response.StatusCode, response);
        }
        catch (Exception ex)
        {
            return StatusCode(500,
                new
                {
                    Success = false, Message = "An error occurred while retrieving the user's roles and domains.",
                    Details = ex.Message
                });
        }
    }

    [HttpDelete("remove-role-domain")]
    public async Task<IActionResult> RemoveRoleFromUserInDomain(RemoveUserRoleDomainDtos removeUserRoleDomainDtos)
    {
        try
        {
            var response = await _userDomainRoleService.RemoveRoleFromUserInDomainAsync(removeUserRoleDomainDtos);
            return StatusCode(response.StatusCode, response);

        }
        catch (Exception ex)
        {
            return StatusCode(500,
                new
                {
                    Success = false, Message = "An error occurred while removing the role from the user in the domain.",
                    Details = ex.Message
                });
        }
    }

    [HttpGet("get-users-by-domain")]
    public async Task<IActionResult> GetAllDomainusersByDomainName(string domainName)
    {
        try
        {
            var response = await _userDomainRoleService.GetAllDomainusersByDomainNameAsync(domainName);
            return StatusCode(response.StatusCode, response);

        }
        catch (Exception ex)
        {
            return StatusCode(500,
                new
                {
                    Success = false, Message = "An error occurred while retrieving users for the specified domain.",
                    Details = ex.Message
                }); 
        }
    }

}