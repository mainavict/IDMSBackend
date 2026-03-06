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

}