namespace IDMSBackend.Models;

public class UserDomainRole
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid RoleId { get; set; }
    public string Role { get; set; } = null!;

    public Guid DomainId { get; set; }
    public String Domain { get; set; } = null!;
    
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
}