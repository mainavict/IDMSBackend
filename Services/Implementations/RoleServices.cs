using IDMSBackend.Services.Interfaces;
using IDMSBackend.Wrappers;
using IDMSBackend.DTOs;
using IDMSBackend.Models;
using IDMSBackend.Data;
using Microsoft.EntityFrameworkCore;

namespace IDMSBackend.Services.Implementations;

public class RoleServices:IRoleServices
{
    
    private ILogger<RoleServices> _logger;
    private readonly AppDbContext _context;
    
    public RoleServices(ILogger<RoleServices> logger, AppDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<ApiResponse<bool>> CreateRoleAsync(CreateRoleDto createRoleDto)
    {
        if (string.IsNullOrWhiteSpace(createRoleDto.Name))
        {
            _logger.LogWarning("Attempted to create a role with an empty name.");
            return  ApiResponse<bool>.SuccessResponse(false, "Role name cannot be empty.", 400);
        }
        
        var standardizedRoleName = 
            char.ToUpper(createRoleDto.Name.Trim()[0]) + 
            createRoleDto.Name.Trim().Substring(1).ToLower();

        var existingRole =await _context.Roles.FirstOrDefaultAsync(r => r.Name == standardizedRoleName);
        if (existingRole != null)
        {
            _logger.LogWarning("Attempted to create a role that already exists: {RoleName}", createRoleDto.Name);
            return  ApiResponse<bool>.SuccessResponse(false,"Role already exists.", 400);
           
        }

        var newRole = new Roles
        {
            Name = standardizedRoleName,
            Description = string.Empty
        };

        await _context.Roles.AddAsync(newRole);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created new role: {RoleName}", standardizedRoleName);
        return ApiResponse<bool>.SuccessResponse(true,"Role created successfully.", 201);
       
    }

    public async  Task<ApiResponse<bool>> DeleteRoleAsync(Guid roleId)
    {
        _logger.LogInformation("Attempting to delete role with ID: {RoleId}", roleId);
        var role = await _context.Roles.FindAsync(roleId);
        if (role == null)
        {
            _logger.LogWarning("Role with ID {RoleId} not found for deletion.", roleId);
            return (new ApiResponse<bool>
            {
                Success = false,
                Message = "Role not found.",
                Data = false
            });
        }
        
         _context.Roles.Remove (role);
        await _context.SaveChangesAsync();
        return ApiResponse<bool>.SuccessResponse(true,"Role deleted successfully.", 200);
       
        
    }
    
    public async Task<ApiResponse<List<RoleResponseDto>>> GetAllRolesAsync()
    {
        var roles = await _context.Roles
            .AsNoTracking()
            .Select(r => new RoleResponseDto
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description
            })
            .ToListAsync();

        return  ApiResponse<List<RoleResponseDto>>.SuccessResponse(roles,"Roles retrieved successfully.", 200);
    }
}