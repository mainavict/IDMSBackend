using System;
using IDMSBackend.Data;
using  IDMSBackend.Services.Interfaces;
using IDMSBackend.DTOs;
using BCrypt.Net;
using IDMSBackend.Models;
using IDMSBackend.Wrappers;
using Microsoft.EntityFrameworkCore;

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
            //checking if  the   schoolId or email already exists in the user table
            if (userCreateDto.userType == UserType.Student && !string.IsNullOrWhiteSpace(userCreateDto.SchoolId))
            {
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.SchoolId == userCreateDto.SchoolId);
                if (existingUser != null)
                {
                    if (existingUser.Status == UserStatus.Deleted)
                    {
                        existingUser.Status = UserStatus.Active;
                        existingUser.FirstName =  existingUser.FirstName;
                        existingUser.LastName = existingUser.LastName;
                        existingUser.Email = existingUser.Email;
                        existingUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(userCreateDto.Password);
                        existingUser.UpdatedAt = DateTime.UtcNow;
                        await _context.SaveChangesAsync();
                        
                        var response = new UserResponseDto
                        {
                            Id = existingUser.Id,
                            SchoolId = existingUser.SchoolId,
                            FacultyId = existingUser.FacultyId,
                            FirstName = existingUser.FirstName,
                            LastName = existingUser.LastName,
                            UserType= existingUser.userType.ToString(),
                            Status = existingUser.Status.ToString(),
                            Email = existingUser.Email,
                            Roles = existingUser.UserDomainRoles.Select(ur => new UserRoleDto
                            {
                                RoleId = ur.RoleId,
                                RoleName = ur.Role.Name,
                                DomainId = ur.DomainId,
                                DomainName = ur.Domain.Name
                            }).ToList()
                        };
                        
                        return  ApiResponse<UserResponseDto>.SuccessResponse(response, "User reactivated successfully", 200);
                    }
                    
                    _logger.LogWarning("User creation failed: User with SchoolId {SchoolId} already exists",
                        userCreateDto.SchoolId);
                    return ApiResponse<UserResponseDto>.FailureResponse("User with this SchoolId already exists", 400);
                }
            }
            //checking if email already exists in the user table
            
            if (!string.IsNullOrWhiteSpace(userCreateDto.Email))
            {
               var existingEmailuser =await  _context.Users.FirstOrDefaultAsync(u => u.Email == userCreateDto.Email);
                          if (existingEmailuser != null)            {

                              if (existingEmailuser.Status == UserStatus.Deleted)
                              {
                                  existingEmailuser.Status = UserStatus.Active;
                                    existingEmailuser.FirstName =  existingEmailuser.FirstName;
                                    existingEmailuser.LastName = existingEmailuser.LastName;
                                    existingEmailuser.Email = existingEmailuser.Email;
                                    existingEmailuser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(userCreateDto.Password);
                                    existingEmailuser.UpdatedAt = DateTime.UtcNow;
                                    await _context.SaveChangesAsync();
                                    
                                    var response = new UserResponseDto
                                    {
                                        Id = existingEmailuser.Id,
                                        SchoolId = existingEmailuser.SchoolId,
                                        FacultyId = existingEmailuser.FacultyId,
                                        FirstName = existingEmailuser.FirstName,
                                        LastName = existingEmailuser.LastName,
                                        UserType= existingEmailuser.userType.ToString(),
                                        Status = existingEmailuser.Status.ToString(),
                                        Email = existingEmailuser.Email,
                                        Roles = existingEmailuser.UserDomainRoles.Select(ur => new UserRoleDto
                                        {
                                            RoleId = ur.RoleId,
                                            RoleName = ur.Role.Name,
                                            DomainId = ur.DomainId,
                                            DomainName = ur.Domain.Name
                                        }).ToList()
                                    };
                                    
                                    return  ApiResponse<UserResponseDto>.SuccessResponse(response, "User reactivated successfully", 200);
                              }
                              
                              _logger.LogWarning("User creation failed: User with Email {Email} already exists",
                                  userCreateDto.Email);
                              return ApiResponse<UserResponseDto>.FailureResponse("User with this Email already exists", 400);
                          }
            }
           
            
            
            
            
            //check if the user is a student and if the schoolId exists in the student table
            if  (userCreateDto.userType == UserType.Student)
             {
                 var existingStudent =await  _context.Students.FirstOrDefaultAsync(s => s.SchoolId == userCreateDto.SchoolId);
                 if (existingStudent == null)
                 {
                     _logger.LogWarning("User creation failed: Student with SchoolId {SchoolId} not found",
                         userCreateDto.SchoolId);
                     return ApiResponse<UserResponseDto>.FailureResponse("Student with this SchoolId does not exists", 400);
                 }
                 
                 

                 var user = new User()
                 {
                     SchoolId = userCreateDto.SchoolId,
                     FirstName = existingStudent.FullName,
                     LastName = existingStudent.FullName,
                     Email = existingStudent.Email,
                     userType = userCreateDto.userType,
                     PasswordHash = BCrypt.Net.BCrypt.HashPassword(userCreateDto.Password),
                 };
                 
                 
                 _logger.LogInformation("Creating user for student with SchoolId: {SchoolId}", userCreateDto.SchoolId);
                    await _context.Users.AddAsync(user);
                    await _context.SaveChangesAsync();
                    
                    var userResponse = new UserResponseDto
                    {
                        Id = user.Id,
                        SchoolId = user.SchoolId,
                        FacultyId = user.FacultyId,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        UserType= user.userType.ToString(),
                        Status = user.Status.ToString(),
                        Email = user.Email,
                        Roles = user.UserDomainRoles.Select(ur => new UserRoleDto
                        {
                            RoleId = ur.RoleId,
                            RoleName = ur.Role.Name,
                            DomainId = ur.DomainId,
                            DomainName = ur.Domain.Name
                        }).ToList()
                    };
                    
                    
                    return ApiResponse<UserResponseDto>.SuccessResponse(userResponse, "User created successfully for student", 201);


             }
            
          
            if (userCreateDto.userType == UserType.Faculty)
            {
                if (string.IsNullOrWhiteSpace(userCreateDto.FirstName) || string.IsNullOrWhiteSpace(userCreateDto.LastName) || string.IsNullOrWhiteSpace(userCreateDto.Email) || string.IsNullOrWhiteSpace(userCreateDto.Password) || string.IsNullOrWhiteSpace(userCreateDto.FacultyId))
                {
                    return ApiResponse<UserResponseDto>.FailureResponse("FirstName, LastName, Email, Password and FacultyId are required fields for faculty users", 400);
                }

            
                var  newuser = new Models.User
                {
                    FacultyId = userCreateDto.FacultyId,
                    FirstName = userCreateDto.FirstName,
                    LastName = userCreateDto.LastName,
                    Email = userCreateDto.Email,
                    userType = userCreateDto.userType,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(userCreateDto.Password)
                };

                await _context.Users.AddAsync(newuser);
                await _context.SaveChangesAsync();

                var usersResponse = new UserResponseDto
                {
                    Id = newuser.Id,
                    FacultyId = newuser.FacultyId,
                    FirstName = newuser.FirstName,
                    LastName = newuser.LastName,
                    Email = newuser.Email,
                    UserType= newuser.userType.ToString(),
                    Status = newuser.Status.ToString(),
                    Roles = newuser.UserDomainRoles.Select(ur => new UserRoleDto
                    {
                        RoleId = ur.RoleId,
                        RoleName = ur.Role.Name,
                        DomainId = ur.DomainId,
                        DomainName = ur.Domain.Name
                    }).ToList()
                };

                _logger.LogInformation("User created successfully with Id: {UserId}", newuser.Id);
                return ApiResponse<UserResponseDto>.SuccessResponse(usersResponse, "User created successfully", 201);
                
            }
            
                return ApiResponse<UserResponseDto>.FailureResponse("Invalid user type specified", 400);
            
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating user with SchoolId: {SchoolId}", userCreateDto.SchoolId);
            return ApiResponse<UserResponseDto>.FailureResponse($"An error occurred while creating the user error:{ex.Message}", 500);
        }
        
        
    }
    
    
    public  async Task<ApiResponse<UserResponseDto>> GetUserByIdAsync(Guid userId)
    {
        _logger.LogInformation("Retrieving user with Id: {UserId}", userId);
        try
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User retrieval failed: User with Id {UserId} not found", userId);
                return ApiResponse<UserResponseDto>.FailureResponse("User not found", 404);
            }

            var userResponse = new UserResponseDto
            {
                Id = user.Id,
                SchoolId = user.SchoolId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Roles = user.UserDomainRoles.Select(ur => new UserRoleDto
                {
                    RoleId = ur.RoleId,
                    RoleName = ur.Role.Name,
                    DomainId = ur.DomainId,
                    DomainName = ur.Domain.Name
                }).ToList()
            };

            _logger.LogInformation("User retrieved successfully with Id: {UserId}", user.Id);
            return ApiResponse<UserResponseDto>.SuccessResponse(userResponse, "User retrieved successfully", 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving user with Id: {UserId}", userId);
            return ApiResponse<UserResponseDto>.FailureResponse($"An error occurred while retrieving the user error:{ex.Message}", 500);
        }
    }


    public async  Task<ApiResponse<List<UserResponseDto>>> GetAllUsersAsync()
    {
        _logger.LogInformation("Retrieving all users");
        try
        {           var users = await _context.Users
                .AsNoTracking().Where(u => u.Status == UserStatus.Active || u.Status == UserStatus.Suspended)
                .Select(u => new UserResponseDto
                {
                    Id = u.Id,
                    SchoolId = u.SchoolId,
                    FacultyId = u.FacultyId,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    UserType = u.userType.ToString(),
                    Status = u.Status.ToString(),
                    Email = u.Email,
                    Roles = u.UserDomainRoles.Select(ur => new UserRoleDto
                    {
                        RoleId = ur.RoleId,
                        RoleName = ur.Role.Name,
                        DomainId = ur.DomainId,
                        DomainName = ur.Domain.Name
                    }).ToList()
                })
                .ToListAsync();
            
           
            
            _logger.LogInformation("All users retrieved successfully. Total users: {UserCount}", users.Count);
            return (ApiResponse<List<UserResponseDto>>.SuccessResponse(users, "All users retrieved successfully", 200));
        }
        catch (Exception ex)        {
            _logger.LogError(ex, "Error occurred while retrieving all users");
            return (ApiResponse<List<UserResponseDto>>.FailureResponse($"An error occurred while retrieving users error:{ex.Message}", 500));
        }
    }
    
    public async Task <ApiResponse<UserResponseDto>> UpdateUserAsync(Guid userId, UserUpdateDto userUpdateDto)
    {
        _logger.LogInformation("updating user with Id: {UserId}", userId);
        try
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User update failed: User with Id {UserId} not found", userId);
                return (ApiResponse<UserResponseDto>.FailureResponse("User not found", 404));
            }

            if (!string.IsNullOrWhiteSpace(userUpdateDto.FirstName))
            {
                user.FirstName = userUpdateDto.FirstName;
            }
            
            if (!string.IsNullOrWhiteSpace(userUpdateDto.LastName))
            {
                user.LastName = userUpdateDto.LastName;
            }

            if (!string.IsNullOrWhiteSpace(userUpdateDto.Email))
            {
                user.Email = userUpdateDto.Email;
            }
            
            await _context.SaveChangesAsync();

            var userResponse = new UserResponseDto
            {
                Id = user.Id,
                SchoolId = user.SchoolId,
                FacultyId = user.FacultyId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                UserType= user.userType.ToString(),
                Status = user.Status.ToString(),
                Roles = user.UserDomainRoles.Select(ur => new UserRoleDto
                {
                    RoleId = ur.RoleId,
                    RoleName = ur.Role.Name,
                    DomainId = ur.DomainId,
                    DomainName = ur.Domain.Name
                }).ToList()
            };

            _logger.LogInformation("User updated successfully with Id: {UserId}", user.Id);
            return (ApiResponse<UserResponseDto>.SuccessResponse(userResponse, "User updated successfully", 200));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating user with Id: {UserId}", userId);
            return (ApiResponse<UserResponseDto>.FailureResponse($"An error occurred while updating the user error:{ex.Message}", 500));
            
        }
    }
    
    
    public  async Task<ApiResponse<bool>> ChangePasswordAsync(Guid userId, ChangePasswordDto changePasswordDto)
    {
        _logger.LogInformation("Changing password for user with Id: {UserId}", userId);
        try
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Password change failed: User with Id {UserId} not found", userId);
                return ApiResponse<bool>.FailureResponse("User not found", 404);
            }

            if (!BCrypt.Net.BCrypt.Verify(changePasswordDto.CurrentPassword, user.PasswordHash))
            {
                _logger.LogWarning("Password change failed: Current password is incorrect for user with Id {UserId}", userId);
                return ApiResponse<bool>.FailureResponse("Current password is incorrect", 400);
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(changePasswordDto.NewPassword);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Password changed successfully for user with Id: {UserId}", user.Id);
            return ApiResponse<bool>.SuccessResponse(true, "Password changed successfully", 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while changing password for user with Id: {UserId}", userId);
            return ApiResponse<bool>.FailureResponse($"An error occurred while changing the password error:{ex.Message}", 500);
        }
    }
    
    
    public async Task<ApiResponse<bool>> ChangeUserStatusAsync(Guid userId,ChangeUserStatusDto changeUserStatusDto)
    {
        _logger.LogInformation("Deleting user with Id: {UserId}", userId);
        try
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User deletion failed: User with Id {UserId} not found", userId);
                return ApiResponse<bool>.FailureResponse("User not found", 404);
            }
            user.Status = changeUserStatusDto.NewStatus;
            await _context.SaveChangesAsync();

            _logger.LogInformation("User deleted successfully with Id: {UserId}", user.Id);
            return ApiResponse<bool>.SuccessResponse(true, "User deleted successfully", 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting user with Id: {UserId}", userId);
            return ApiResponse<bool>.FailureResponse($"An error occurred while deleting the user error:{ex.Message}", 500);
        }
    }
    
    
}