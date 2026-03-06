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
    
    public async Task <ApiResponse<List<UserDomainRoleDtos>>> GetUserRolesAndDomainsAsync(Guid userId)
    {
        _logger.LogInformation("Retrieving user roles and domains for user ID: {UserId}", userId);

        try
        {
            var user = await _dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                _logger.LogWarning("User not found with ID: {UserId}", userId);
                return  ApiResponse<List<UserDomainRoleDtos>>.FailureResponse("User not found", 404);
            }
            
            var userDomainRoles = await _dbContext.UserDomainRoles
                .Where(udr => udr.UserId == userId)
                .Include(udr => udr.Role)
                .Include(udr => udr.Domain)
                .AsNoTracking()
                .ToListAsync();

            var userDomainRoleDtos = userDomainRoles.Select(udr => new UserDomainRoleDtos
            {
                UserId = udr.UserId,
                RoleId = udr.RoleId,
                RoleName = udr.Role.Name,
                DomainId = udr.DomainId,
                DomainName = udr.Domain.Name,
    }
            ).ToList();

            _logger.LogInformation("User roles and domains retrieved successfully for user ID: {UserId}", userId);
            return  ApiResponse<List<UserDomainRoleDtos>>.SuccessResponse(userDomainRoleDtos, "User roles and domains retrieved successfully", 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving user roles and domains for user ID: {UserId}", userId);
            return  ApiResponse<List<UserDomainRoleDtos>>.FailureResponse("An error occurred while retrieving the user roles and domains.", 500);
        }
    }

    public async Task<ApiResponse<bool>> RemoveRoleFromUserInDomainAsync(RemoveUserRoleDomainDtos removeRoleFromUserInDomainDtos)
    {
        _logger.LogInformation("Removing role from user in domain");

        try
        {
            var user = await _dbContext.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == removeRoleFromUserInDomainDtos.UserId);
            if (user == null)
            {
                _logger.LogWarning("User not found with ID: {UserId}", removeRoleFromUserInDomainDtos.UserId);
                return ApiResponse<bool>.FailureResponse("User not found", 404);
            }

            var role = await _dbContext.Roles.FirstOrDefaultAsync(r => r.Name == removeRoleFromUserInDomainDtos.Role);
            if (role == null)
            {
                _logger.LogWarning("Role not found with name: {RoleName}", removeRoleFromUserInDomainDtos.Role);
                return ApiResponse<bool>.FailureResponse("Role not found", 404);
            }

            var domain =
                await _dbContext.Domains.FirstOrDefaultAsync(d => d.Name == removeRoleFromUserInDomainDtos.Domain);
            if (domain == null)
            {
                _logger.LogWarning("Domain not found with name: {DomainName}", removeRoleFromUserInDomainDtos.Domain);
                return ApiResponse<bool>.FailureResponse("Domain not found", 404);
            }

            var userDomainRole = await _dbContext.UserDomainRoles.FirstOrDefaultAsync(udr =>
                udr.UserId == removeRoleFromUserInDomainDtos.UserId &&
                udr.RoleId == role.Id &&
                udr.DomainId == domain.Id);

            if (userDomainRole == null)
            {
                _logger.LogWarning("User does not have the role assigned in the domain");
                return ApiResponse<bool>.FailureResponse("User does not have the role assigned in the domain", 400);
            }

            _dbContext.UserDomainRoles.Remove(userDomainRole);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Role removed from user in domain successfully");
            return ApiResponse<bool>.SuccessResponse(true, "Role removed from user in domain successfully", 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while removing role from user in domain");
            return ApiResponse<bool>.FailureResponse(
                "An error occurred while removing the role from the user in the domain.", 500);
        }
    }


}