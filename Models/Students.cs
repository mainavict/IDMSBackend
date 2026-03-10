using  System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace IDMSBackend.Models;

public class Students
{
    [Key]
    public Guid Id { get; set; }= Guid.NewGuid();
    [Required]
    [StringLength(100)]
    public string SchoolId { get; set; } = string.Empty;
    
    [Required]
    [StringLength(200)]
    public string FullName { get; set; }= string.Empty;
    [StringLength(200)]
    public string  Email { get; set; } = string.Empty;
    [StringLength(100)]
    public string  YearOfStudy { get; set; } = string.Empty;
    [StringLength(100)]
    public string Residence { get; set; } = string.Empty;
    [StringLength(100)]
    public string AcademicStatus { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; }= DateTime.UtcNow;
    public DateTime LastSyncDate { get; set; }
    
    public StudentCards StudentCards { get; set; } 
    
}