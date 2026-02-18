using IDMSBackend.Services.Interfaces;
using IDMSBackend.DTOs;
using IDMSBackend.Models;
using IDMSBackend.Data;
using IDMSBackend.Wrappers;
using Microsoft.EntityFrameworkCore;

namespace IDMSBackend.Services.Implementations;


public class StudentCardServices:IStudentCards
{
    private readonly AppDbContext _context;
    
    public StudentCardServices(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task<ApiResponse<StudentCardsDto>> CreateStudentCardAsync(CreateStudentCardsDto createDto)
    {
        var student = await _context.Students.FirstOrDefaultAsync(s => s.SchoolId == createDto.SchoolId);
        if (student == null)
        {
            return ApiResponse<StudentCardsDto>.FailureResponse("Student not found", 404);
        }
        
        var existingCard = await _context.StudentCards.FirstOrDefaultAsync(c => c.StudentId == student.Id);
        
        if  (existingCard != null)
        {
           student.StudentCards.CardUuid = createDto.CardUuid;
           student.StudentCards.IsActive = createDto.IsActive;
           student.StudentCards.ExpiryDate = DateTime.Now .AddYears(1);
           _context.StudentCards.Update(student.StudentCards);
           await _context.SaveChangesAsync();
           
           var updatedCardDto = new StudentCardsDto
           {
               SchoolId = student.SchoolId,
               CardUuid = student.StudentCards.CardUuid,
               IsActive = student.StudentCards.IsActive,
               IssuedAt = student.StudentCards.IssuedAt,
               ExpiryDate = student.StudentCards.ExpiryDate,
               RevokedAt = student.StudentCards.RevokedAt
           };
           
           
           return ApiResponse<StudentCardsDto>.SuccessResponse(updatedCardDto, "Student card updated successfully", 201);
            
        }
        
        var studentCard = new StudentCards
        {
            StudentId = student.Id,
            CardUuid = createDto.CardUuid,
            IsActive = createDto.IsActive,
            ExpiryDate = DateTime.UtcNow.AddYears(1)
        };
        
        _context.StudentCards.Add(studentCard);
        await _context.SaveChangesAsync();
        var studentCardDto = new StudentCardsDto
        {
            SchoolId = student.SchoolId,
            CardUuid = studentCard.CardUuid,
            IsActive = studentCard.IsActive,
            IssuedAt = studentCard.IssuedAt,
            ExpiryDate = studentCard.ExpiryDate,
            RevokedAt = studentCard.RevokedAt
        };
        
        return ApiResponse<StudentCardsDto>.SuccessResponse(studentCardDto, "Student card created successfully", 201);
         
    }
    
    public async Task<ApiResponse<StudentCardsDto>> GetStudentCardBySchoolIdAsync(string schoolId)
    {
        var student = await _context.Students.FirstOrDefaultAsync(s => s.SchoolId == schoolId);
        if (student == null)
        {
            return ApiResponse<StudentCardsDto>.FailureResponse("Student not found", 404);
        }
        
        var studentCard = await _context.StudentCards.FirstOrDefaultAsync(c => c.StudentId == student.Id);
        
        if (studentCard == null)
        {
            return ApiResponse<StudentCardsDto>.FailureResponse("Student card not found", 404);
        }
        
        var studentCardDto = new StudentCardsDto
        {
            SchoolId = student.SchoolId,
            FullName = student.FullName,
            CardUuid = studentCard.CardUuid,
            IsActive = studentCard.IsActive,
            IssuedAt = studentCard.IssuedAt,
            ExpiryDate = studentCard.ExpiryDate,
            RevokedAt = studentCard.RevokedAt
        };
        
        return ApiResponse<StudentCardsDto>.SuccessResponse(studentCardDto, "Student card retrieved successfully", 200);
    }

}