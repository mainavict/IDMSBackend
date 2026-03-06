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
                return  ApiResponse<UserDomainRoleDtos>.FailureResponse("Input data cannot be null", 400);
            }

            var user = _dbContext.Users.AsNoTracking().FirstOrDefault(u => u.Id == allocateRoleToUserDtos.UserId);

            if (user == null)
            {
                _logger.LogWarning("User not found with ID: {UserId}", allocateRoleToUserDtos.UserId);
                return  ApiResponse<UserDomainRoleDtos>.FailureResponse("User not found", 404);
            }

            var role = _dbContext.Roles.FirstOrDefault(r => r.Name == allocateRoleToUserDtos.Role);
            if (role == null)
            {
                _logger.LogWarning("Role not found with name: {RoleName}", allocateRoleToUserDtos.Role);
                return ApiResponse<UserDomainRoleDtos>.FailureResponse("Role not found", 404);
            }

            var domain = _dbContext.Domains.FirstOrDefault(d => d.Name == allocateRoleToUserDtos.Domain);
            if (domain == null)
            {
                _logger.LogWarning("Domain not found with name: {DomainName}", allocateRoleToUserDtos.Domain);
                return  ApiResponse<UserDomainRoleDtos>.FailureResponse("Domain not found", 404);

            }
            
            var  existingAssignment = _dbContext.UserDomainRoles.FirstOrDefault(udr =>
                udr.UserId == allocateRoleToUserDtos.UserId &&
                udr.RoleId == role.Id &&
                udr.DomainId == domain.Id);
            if (existingAssignment != null)            {
                _logger.LogWarning("User already has the role assigned in the domain");
                return  ApiResponse<UserDomainRoleDtos>.FailureResponse("User already has the role assigned in the domain", 400);
            }

            var userDomainRole = new UserDomainRole
            {
                UserId = allocateRoleToUserDtos.UserId,
                RoleId = role.Id,
                DomainId = domain.Id,
                AssignedAt = DateTime.UtcNow
               

            };

            _dbContext.UserDomainRoles.Add(userDomainRole);
            await _dbContext.SaveChangesAsync();

            var userDomainRoleDto = new UserDomainRoleDtos
            {
                UserId = userDomainRole.UserId,
                FullName = user.FirstName + " " + user.LastName,
                RoleId = userDomainRole.RoleId,
                RoleName = role.Name,
                DomainId = userDomainRole.DomainId,
                DomainName = domain.Name,
                AssignedAt = userDomainRole.AssignedAt
            };

            return  ApiResponse<UserDomainRoleDtos>.SuccessResponse(userDomainRoleDto, "Role assigned to user in domain successfully", 200);
            _logger.LogInformation("Role assigned to user in domain successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while assigning role to user in domain");
            return  ApiResponse<UserDomainRoleDtos>.FailureResponse("An error occurred while assigning the role to the user in the domain.", 500);
          
        }

    }
}