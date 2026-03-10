using IDMSBackend.DTOs;
using IDMSBackend.Wrappers;

namespace IDMSBackend.Services.Interfaces;

public interface IUserService
{
    Task <ApiResponse<UserResponseDto>> CreateUserAsync(UserCreateDto userCreateDto);
    Task <ApiResponse<UserResponseDto>> GetUserByIdAsync(Guid userId);
    Task <ApiResponse<List<UserResponseDto>>> GetAllUsersAsync();
    Task <ApiResponse<UserResponseDto>> UpdateUserAsync(Guid userId, UserUpdateDto userUpdateDto);
    Task <ApiResponse<bool>> ChangePasswordAsync(Guid userId, ChangePasswordDto changePasswordDto);
    Task <ApiResponse<bool>> ChangeUserStatusAsync(Guid userId, ChangeUserStatusDto changeUserStatusDto);
     // Task <ApiResponse<bool>> AssignRoleToUserAsync(Guid userId, Guid roleId, Guid domainId);
     // Task <ApiResponse<bool>> RemoveRoleFromUserAsync(Guid userId, Guid roleId, Guid domainId);
    
    
    
}