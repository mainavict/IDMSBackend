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
    private readonly ILogger<StudentCardServices> _logger;
    
    public StudentCardServices(AppDbContext context,ILogger<StudentCardServices> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    public async Task<ApiResponse<StudentCardsDto>> CreateStudentCardAsync(CreateStudentCardsDto createDto)
    {
        _logger.LogInformation("Attempting to create or update student card for SchoolId: {SchoolId}", createDto.SchoolId);

        try
        {
            var student = await _context.Students.FirstOrDefaultAsync(s => s.SchoolId == createDto.SchoolId);
            if (student == null)
            {
                _logger.LogWarning("Card creation failed: Student with SchoolId {SchoolId} not found",
                    createDto.SchoolId);
                return ApiResponse<StudentCardsDto>.FailureResponse("Student not found", 404);
            }

            var existingCard = await _context.StudentCards.FirstOrDefaultAsync(c => c.StudentId == student.Id);

            if (existingCard != null)
            {
                _logger.LogInformation("updating existing card for SchoolId: {SchoolId}", createDto.SchoolId);
                student.StudentCards.CardUuid = createDto.CardUuid;
                student.StudentCards.IsActive = createDto.IsActive;
                student.StudentCards.ExpiryDate = DateTime.Now.AddYears(1);
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


                return ApiResponse<StudentCardsDto>.SuccessResponse(updatedCardDto, "Student card updated successfully",
                    201);

            }

            _logger.LogInformation("Creating new card for SchoolId: {SchoolId}", createDto.SchoolId);

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

            return ApiResponse<StudentCardsDto>.SuccessResponse(studentCardDto, "Student card created successfully",
                201);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while creating/updating student card for SchoolId: {SchoolId}", createDto.SchoolId);
            return ApiResponse<StudentCardsDto>.FailureResponse("An error occurred while processing the request", 500);
        }
    }
    
    public async Task<ApiResponse<StudentCardsDto>> GetStudentCardBySchoolIdAsync(string schoolId)
    {
        _logger.LogInformation("Retrieving student card for SchoolId: {SchoolId}", schoolId);
        var student = await _context.Students.FirstOrDefaultAsync(s => s.SchoolId == schoolId);
        if (student == null)
        {
            _logger.LogWarning("Setudent card retrieval failed: Student with SchoolId {SchoolId} not found", schoolId);
            return ApiResponse<StudentCardsDto>.FailureResponse("Student not found", 404);
        }
        
        var studentCard = await _context.StudentCards.FirstOrDefaultAsync(c => c.StudentId == student.Id);
        
        if (studentCard == null)
        {
            _logger.LogWarning("Student card retrieval failed: No card found for SchoolId {SchoolId}", schoolId);
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
        
        _logger.LogInformation("Retrieving student card for SchoolId: {SchoolId}", schoolId);
        return ApiResponse<StudentCardsDto>.SuccessResponse(studentCardDto, "Student card retrieved successfully", 200);
    }

}