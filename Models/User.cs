using System.ComponentModel.DataAnnotations;

namespace IDMSBackend.Models;

public enum UserType
{
    Student,
    Faculty
}

public enum UserStatus
{
    Active,      // Normal, can login and use the app
    Suspended,   // Temporarily blocked, cannot login
    Deleted      // Soft-deleted, hidden from normal queries
}


public class User
{
    [Key]
    public Guid Id { get; set; }= Guid.NewGuid();
    
    
    
    [StringLength(100)]
    public string? SchoolId { get; set; } 
    
   
    [StringLength(100)]
    public string? FacultyId { get; set; } 
    
    public UserType userType { get; set; }
    
    [Required]
    public string FirstName { get; set; }
    [Required]
    public string LastName{ get; set; }
    
    public UserStatus Status { get; set; } = UserStatus.Active;
    public  string  PasswordHash { get; set; } = string.Empty;
    public string  Email { get; set; } = string.Empty;
   
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; }= DateTime.UtcNow;
    public ICollection<Events> CreatedEvents { get; set; } = new List<Events>();
    public ICollection<Events> UpdatedEvents { get; set; } = new List<Events>();
    public ICollection<UserDomainRole> UserDomainRoles { get; set; }
        = new List<UserDomainRole>();

   

    
}