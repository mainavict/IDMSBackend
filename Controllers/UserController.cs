using IDMSBackend.DTOs;
using IDMSBackend.Services.Interfaces;
using IDMSBackend.Models;
using Microsoft.AspNetCore.Mvc;

namespace IDMSBackend.Controllers;
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    
    public UserController(IUserService userService)
    {
        _userService = userService;
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateUser(UserCreateDto userCreateDto)
    {
        try
        {
            var response = await _userService.CreateUserAsync(userCreateDto);
            return StatusCode(response.StatusCode, response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Success = false, Message = "An error occurred while creating the user.", Details = ex.Message });
        }
    }
    
}