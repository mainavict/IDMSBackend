using IDMSBackend.Data;
using IDMSBackend.DTOs;
using IDMSBackend.Services.Interfaces;
using IDMSBackend.Wrappers;
using  Microsoft.EntityFrameworkCore;
using IDMSBackend.Models;
namespace IDMSBackend.Services.Implementations;


public class UserDomainRoleServices : IUserDomainRole
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<StudentServices> _logger;

    public UserDomainRoleServices(AppDbContext dbContext, ILogger<StudentServices> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<ApiResponse<UserDomainRoleDtos>> AssignRoleDomainAsync(
        AllocateRoleToUserDtos allocateRoleToUserDtos)
    {
        _logger.LogInformation("Assigning role to user in domain");

        try
        {
            _logger.LogInformation("Validating input data");
            if (allocateRoleToUserDtos == null)
            {
                _logger.LogWarning("Input data is null");
                return new ApiResponse<UserDomainRoleDtos>
                {
                    Success = false,
                    Message = "Input data cannot be null",
                    Data = null
                };
            }

            var user = _dbContext.Users.AsNoTracking().FirstOrDefault(u => u.Id == allocateRoleToUserDtos.UserId);

            if (user == null)
            {
                _logger.LogWarning("User not found with ID: {UserId}", allocateRoleToUserDtos.UserId);
                return new ApiResponse<UserDomainRoleDtos>
                {
                    Success = false,
                    Message = "User not found",
                    Data = null
                };
            }

            var role = _dbContext.Roles.AsNoTracking().FirstOrDefault(r => r.Name == allocateRoleToUserDtos.Role);
            if (role == null)
            {
                _logger.LogWarning("Role not found with name: {RoleName}", allocateRoleToUserDtos.Role);
                return new ApiResponse<UserDomainRoleDtos>
                {
                    Success = false,
                    Message = "Role not found",
                    Data = null
                };
            }

            var domain = _dbContext.Domains.AsNoTracking().FirstOrDefault(d => d.Name == allocateRoleToUserDtos.Domain);
            if (domain == null)
            {
                _logger.LogWarning("Domain not found with name: {DomainName}", allocateRoleToUserDtos.Domain);
                return new ApiResponse<UserDomainRoleDtos>
                {
                    Success = false,
                    Message = "Domain not found",
                    Data = null
                };

            }

            var userDomainRole = new UserDomainRole
            {
                UserId = allocateRoleToUserDtos.UserId,
                User = user,
                RoleId = role.Id,
                Role = role.Name,
                DomainId = domain.Id,
                Domain = domain.Name

            };

            _dbContext.UserDomainRoles.Add(userDomainRole);
            await _dbContext.SaveChangesAsync();

            var userDomainRoleDto = new UserDomainRoleDtos
            {
                UserId = userDomainRole.UserId,
                FullName = user.FirstName + " " + user.LastName,
                RoleId = userDomainRole.RoleId,
                RoleName = userDomainRole.Role,
                DomainId = userDomainRole.DomainId,
                DomainName = userDomainRole.Domain,
                AssignedAt = userDomainRole.AssignedAt
            };

            return new ApiResponse<UserDomainRoleDtos>
            {
                Success = true,
                Message = "Role assigned to user in domain successfully",
                Data = userDomainRoleDto
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while assigning role to user in domain");
            return new ApiResponse<UserDomainRoleDtos>
            {
                Success = false,
                Message = "An error occurred while assigning role to user in domain",
                Data = null
            };
        }

    }
}