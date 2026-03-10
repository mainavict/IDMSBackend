using IDMSBackend.Wrappers;
using IDMSBackend.DTOs;
namespace IDMSBackend.Services.Interfaces;

public interface IRoleServices
{
    public Task<ApiResponse<bool>> CreateRoleAsync(CreateRoleDto createRoleDto);
    public Task<ApiResponse<bool>> DeleteRoleAsync(Guid roleId);
    public Task<ApiResponse<List<RoleResponseDto>>>GetAllRolesAsync();
}