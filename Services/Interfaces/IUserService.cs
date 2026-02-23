using IDMSBackend.DTOs;
using IDMSBackend.Wrappers;

namespace IDMSBackend.Services.Interfaces;

public interface IUserService
{
    Task <ApiResponse<UserResponseDto>> CreateUserAsync(UserCreateDto userCreateDto);
    // Task <UserResponseDto> GetUserByIdAsync(Guid userId);
    // Task <IEnumerable<UserResponseDto>> GetAllUsersAsync();
    // Task <UserResponseDto> UpdateUserAsync(Guid userId, UserUpdateDto userUpdateDto);
    // Task <bool> DeleteUserAsync(Guid userId);
    // Task <bool> ChangePasswordAsync(Guid userId, ChangePasswordDto changePasswordDto);
}