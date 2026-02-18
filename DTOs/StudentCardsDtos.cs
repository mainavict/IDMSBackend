using System.ComponentModel.DataAnnotations;

namespace IDMSBackend.DTOs;

public class CreateStudentCardsDto
{
    [Required]
    public string  SchoolId { get; set; }= string.Empty;
    [Required]
    public string CardUuid { get; set; } = string.Empty;
    public bool IsActive { get; set; }= true;
    
    
    
}

public class UpdateStudentCardsDto
{
    [Required]
    public string  SchoolId { get; set; }= string.Empty;
    
    public string CardUuid { get; set; } = string.Empty;
    
    public bool IsActive { get; set; }
    
    public DateTime IssuedAt { get; set; }
    
    public DateTime ExpiryDate { get; set; }
}

public class StudentCardsDto
{
    public string  SchoolId { get; set; }= string.Empty;
    
    public string FullName { get; set; }= string.Empty;
    
    public string CardUuid { get; set; } = string.Empty;
    
    public bool IsActive { get; set; }
    
    public DateTime IssuedAt { get; set; }
    
    public DateTime ExpiryDate { get; set; }
    
    public DateTime RevokedAt { get; set; }
}