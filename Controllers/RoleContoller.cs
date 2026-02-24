using IDMSBackend.DTOs;
using Microsoft.AspNetCore.Mvc;
using  IDMSBackend.Services.Interfaces;


namespace IDMSBackend.Controllers;
[ApiController]
[Route("api/[controller]")]
public class RoleContoller:ControllerBase
{
    private readonly IRoleServices _roleServices;
    
    public RoleContoller(IRoleServices roleServices)
    {
        _roleServices = roleServices;
    }

    [HttpPost]
    public async Task<IActionResult> CreateRole(CreateRoleDto createRoleDto)
    {
        try
        {
            var response = await _roleServices.CreateRoleAsync(createRoleDto);
            return StatusCode(response.StatusCode, response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Success = false, Message = "An error occurred while creating the role.", Details = ex.Message });
        }
    }
    

    [HttpDelete("{roleId}")]
    public async Task<IActionResult> DeleteRole(Guid roleId)
    {
        try
        {
            var response = await _roleServices.DeleteRoleAsync(roleId);
            return StatusCode(response.StatusCode, response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Success = false, Message = "An error occurred while deleting the role.", Details = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAllRoles()
    {
        try
        {
            var response = await _roleServices.GetAllRolesAsync();
            return StatusCode(response.StatusCode, response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Success = false, Message = "An error occurred while retrieving roles.", Details = ex.Message });
        }
    }

}