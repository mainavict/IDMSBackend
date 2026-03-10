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
    
    [HttpGet("{userId}")]
    public  async Task<IActionResult> GetUserById(Guid userId)
    {
        try
        {
            var response = await _userService.GetUserByIdAsync(userId);
            return StatusCode(response.StatusCode, response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Success = false, Message = "An error occurred while retrieving the user.", Details = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        try
        {
            var response = await _userService.GetAllUsersAsync();
            return StatusCode(response.StatusCode, response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Success = false, Message = "An error occurred while retrieving users.", Details = ex.Message });
        }
    }
    
    [HttpPut("{userId}")]
    public async Task<IActionResult> UpdateUser(Guid userId, UserUpdateDto userUpdateDto)
    {
        try
        {
            var response = await _userService.UpdateUserAsync(userId, userUpdateDto);
            return StatusCode(response.StatusCode, response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Success = false, Message = "An error occurred while updating the user.", Details = ex.Message });
        }
    }

    [HttpPost("{userId}/change-password")]
    public async Task<IActionResult> ChangePassword(Guid userId, ChangePasswordDto changePasswordDto)
    {
        try
        {
            var response = await _userService.ChangePasswordAsync(userId, changePasswordDto);
            return StatusCode(response.StatusCode, response);
        }
        catch (Exception ex)
        {
            return StatusCode(500,
                new
                {
                    Success = false, Message = "An error occurred while changing the password.", Details = ex.Message
                });
        }

    }
    
    [HttpPost("{userId}/change-status")]
   public async Task<IActionResult> ChangeUserStatus(Guid userId, ChangeUserStatusDto changeUserStatusDto)
    {
        try
        {
            var response = await _userService.ChangeUserStatusAsync(userId, changeUserStatusDto);
            return StatusCode(response.StatusCode, response);
        }
        catch (Exception ex)
        {
            return StatusCode(500,
                new
                {
                    Success = false, Message = "An error occurred while changing the user status.", Details = ex.Message
                });
        }
    }



}