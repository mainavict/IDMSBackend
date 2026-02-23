using IDMSBackend.DTOs;
using IDMSBackend.Wrappers;

namespace IDMSBackend.Services.Interfaces;

public interface IUserService
{
    Task <ApiResponse<UserResponseDto>> CreateUserAsync(UserCreateDto userCreateDto);
    Task <ApiResponse<UserResponseDto>> GetUserByIdAsync(Guid userId);
    
    Task <ApiResponse<List<UserResponseDto>>> GetAllUsersAsync();
    
    Task <ApiResponse<UserResponseDto>> UpdateUserAsync(Guid userId, UserUpdateDto userUpdateDto);
    
    // Task <bool> DeleteUserAsync(Guid userId);
    // Task <bool> ChangePasswordAsync(Guid userId, ChangePasswordDto changePasswordDto);
}