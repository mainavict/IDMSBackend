using System.ComponentModel.DataAnnotations;

namespace IDMSBackend.Models;

public enum UserRole
{
    Operator,
    Admin,
}

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
    public UserRole Role { get; set; } = UserRole.Operator;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; }= DateTime.UtcNow;
    
}