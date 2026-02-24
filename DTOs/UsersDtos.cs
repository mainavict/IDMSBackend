using  System.ComponentModel.DataAnnotations;
using IDMSBackend.DTOs;
using IDMSBackend.Models;

namespace IDMSBackend.DTOs;

public class UserResponseDto
{
    public Guid Id { get; set; }
    public string SchoolId { get; set; } = string.Empty;
    
    public string? FacultyId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public List<UserRoleDto> Roles { get; set; } = new List<UserRoleDto>();

}

public class UserCreateDto
    {
        public string? SchoolId { get; set; } 
        
        public string? FacultyId { get; set; }

       public UserType userType { get; set; }
       
        public string? FirstName { get; set; } 
        public string? LastName { get; set; } 
        public string? Email { get; set; } 
        public string Password { get; set; } 
    }

public class UserUpdateDto
    {
        [StringLength(100)]
        public string? FirstName { get; set; } = string.Empty;
        [StringLength(100)]
        public string? LastName { get; set; } = string.Empty;
        [EmailAddress]
        public string? Email { get; set; } = string.Empty;
        

    }

public class UserRoleDto
{
    public Guid RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;

    public Guid DomainId { get; set; }
    public string DomainName { get; set; } = string.Empty;
}


public class ChangePasswordDto
{
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string NewPassword { get; set; } = string.Empty;
}

    

