using System.ComponentModel.DataAnnotations;

namespace IDMSBackend.DTOs;

public class RoleResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class CreateRoleDto
{
    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
}

public class UserRoleDetailDto
{
    public Guid UserRoleId { get; set; } // The ID of the relationship
    public Guid RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public DateTime AssignedAt { get; set; }
}