namespace IDMSBackend.DTOs;

public class AllocateRoleToUserDtos
{
    public Guid UserId { get; set; }
    public Guid Role {get; set; }
    public Guid Domain { get; set; }
}

public class UserDomainRoleDtos
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public Guid RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public Guid DomainId { get; set; }
    public string DomainName { get; set; } = string.Empty;
    public DateTime AssignedAt { get; set; }
}


