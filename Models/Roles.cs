using  System.ComponentModel.DataAnnotations;

namespace IDMSBackend.Models;

public class Roles
{
    [Key]
    public Guid Id { get; set; }= Guid.NewGuid();
    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;
    
    public ICollection<UserDomainRole> UserDomainRoles { get; set; }
        = new List<UserDomainRole>();
    
    
}