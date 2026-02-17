using  System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IDMSBackend.Models;

public class StudentCards
{
    [Key]
    public Guid Id { get; set; }= Guid.NewGuid();
    [Required ]
    public Guid StudentId { get; set; }
    public string CardUuid { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiryDate { get; set; } = DateTime.UtcNow;
    public DateTime RevokedAt { get; set; } = DateTime.UtcNow;
    
    [ForeignKey("StudentId")] 
    public Students Student { get; set; } = null!;
    
}