using System.ComponentModel.DataAnnotations;

namespace IDMSBackend.Models;



public class User
{
    [Key]
    public Guid Id { get; set; }= Guid.NewGuid();
    
    
    [Required]
    [StringLength(100)]
    public string SchoolId { get; set; } = string.Empty;
    [Required]
    public string FirstName { get; set; }
    [Required]
    public string LastName{ get; set; }
    public  string  PasswordHash { get; set; } = string.Empty;
    public string  Email { get; set; } = string.Empty;
    public  bool IsStudent { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; }= DateTime.UtcNow;
    
    public ICollection<Events> CreatedEvents { get; set; } = new List<Events>();
    public ICollection<Events> UpdatedEvents { get; set; } = new List<Events>();
    public ICollection<UserDomainRole> UserDomainRoles { get; set; }
        = new List<UserDomainRole>();

   

    
}