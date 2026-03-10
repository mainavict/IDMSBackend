namespace IDMSBackend.Models;

public class Domains
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public ICollection<UserDomainRole> UserDomainRoles { get; set; }
        = new List<UserDomainRole>();

    public ICollection<Events> Events { get; set; }
        = new List<Events>();
    
}