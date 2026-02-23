using System;
using IDMSBackend.Data;
using  IDMSBackend.Services.Interfaces;
using IDMSBackend.DTOs;
using BCrypt.Net;
using IDMSBackend.Wrappers;

namespace IDMSBackend.Services.Implementations;
public class UserServices : IUserService
{
    private readonly ILogger<UserServices> _logger;
    private readonly AppDbContext _context;
    
    public UserServices(AppDbContext context, ILogger<UserServices> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    public async Task<ApiResponse<UserResponseDto>> CreateUserAsync(UserCreateDto userCreateDto)
    {
        _logger.LogInformation("Creating user with SchoolId: {SchoolId}", userCreateDto.SchoolId);
        try
        {
            var existinguser = _context.Users.FirstOrDefault(u => u.SchoolId == userCreateDto.SchoolId);
            if (existinguser != null)
            {
                _logger.LogWarning("User creation failed: User with SchoolId {SchoolId} already exists",
                    userCreateDto.SchoolId);
                return ApiResponse<UserResponseDto>.FailureResponse("User with this SchoolId already exists", 400);
            }
            var existingEmail = _context.Users.FirstOrDefault(u => u.Email == userCreateDto.Email);
            if (existingEmail != null)            {
                _logger.LogWarning("User creation failed: User with Email {Email} already exists",
                    userCreateDto.Email);
                return ApiResponse<UserResponseDto>.FailureResponse("User with this Email already exists", 400);
            }
            
            
            var user = new Models.User
            {
                SchoolId = userCreateDto.SchoolId,
                FirstName = userCreateDto.FirstName,
                LastName = userCreateDto.LastName,
                Email = userCreateDto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(userCreateDto.Password)
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            var userResponse = new UserResponseDto
            {
                Id = user.Id,
                SchoolId = user.SchoolId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                IsActive = true,
                Roles = user.UserDomainRoles.Select(ur => new UserRoleDto
                {
                    RoleId = ur.RoleId,
                    RoleName = ur.Role.Name,
                    DomainId = ur.DomainId,
                    DomainName = ur.Domain.Name
                }).ToList()
            };

            _logger.LogInformation("User created successfully with Id: {UserId}", user.Id);
            return ApiResponse<UserResponseDto>.SuccessResponse(userResponse, "User created successfully", 201);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating user with SchoolId: {SchoolId}", userCreateDto.SchoolId);
            return ApiResponse<UserResponseDto>.FailureResponse($"An error occurred while creating the user error:{ex.Message}", 500);
        }
        
        
    }
    
}