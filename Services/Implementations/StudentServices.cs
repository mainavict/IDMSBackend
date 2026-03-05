using IDMSBackend.Services.Interfaces;
using IDMSBackend.DTOs;
using IDMSBackend.Models;
using IDMSBackend.Wrappers;
using Microsoft.EntityFrameworkCore;
using IDMSBackend.Data;
using Microsoft.EntityFrameworkCore;

namespace IDMSBackend.Services.Implementations;

public class StudentServices: IStudentServices
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<StudentServices> _logger;

    public StudentServices(AppDbContext dbContext, ILogger<StudentServices> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    
    public async Task<ApiResponse<List<StudentResponseDtos>>> GetAllStudentsAsync()
    {
        var students = await _dbContext.Students.AsNoTracking().ToListAsync();
        var studentDtos = students.Select(s => new StudentResponseDtos
        {
            Id = s.Id,
            Name = s.FullName,
            SchoolId = s.SchoolId,
            Email = s.Email,
            YearOfStudy = s.YearOfStudy,
            Residence = s.Residence,
            AcademicStatus = s.AcademicStatus,
            LastSyncDate = s.LastSyncDate
        }).ToList();

        return ApiResponse<List<StudentResponseDtos>>.SuccessResponse(studentDtos, "Students retrieved successfully", 200);
    }
    
    public async Task<ApiResponse<StudentResponseDtos>> GetStudentByIdAsync(String id)
    {
        var student = await _dbContext.Students.AsNoTracking().FirstOrDefaultAsync(s => s.SchoolId == id);
        if (student == null)
        {
            return ApiResponse<StudentResponseDtos>.FailureResponse("Student not found", 404);
        }

        var studentDto = new StudentResponseDtos
        {
            Id = student.Id,
            Name = student.FullName,
            SchoolId = student.SchoolId,
            Email = student.Email,
            YearOfStudy = student.YearOfStudy,
            Residence = student.Residence,
            AcademicStatus = student.AcademicStatus,
            LastSyncDate = student.LastSyncDate
        };

        return ApiResponse<StudentResponseDtos>.SuccessResponse(studentDto, "Student retrieved successfully", 200);
    }
    
    
    public async Task<ApiResponse<SyncResultDto>> SyncStudentsAsync(List<StudentSyncDto> externalStudents)
    {
        _logger.LogInformation("IDMS Sync: Processing {Count} students from Master DB", externalStudents.Count);
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        // 1. Load existing students into memory (SchoolId as key) to avoid N+1 query problem
        var localStudents = await _dbContext.Students.ToDictionaryAsync(s => s.SchoolId);
        
        int added = 0;
        int updated = 0;

        foreach (var ext in externalStudents)
        {
            if (localStudents.TryGetValue(ext.SchoolId, out var existing))
            {
                // Update if data changed
                if (existing.YearOfStudy != ext.YearOfStudy || existing.Email != ext.Email ||existing.FullName != ext.FullName || existing.Residence != ext.Residence || existing.AcademicStatus != ext.AcademicStatus)
                {
                    existing.FullName = ext.FullName;
                    existing.Email = ext.Email;
                    existing.Residence = ext.Residence;
                    existing.YearOfStudy = ext.YearOfStudy;
                    existing.AcademicStatus = ext.AcademicStatus;
                    existing.UpdatedAt = DateTime.UtcNow;
                    existing.LastSyncDate= DateTime.UtcNow;
                    updated++;
                }
                else
                {
                    existing.LastSyncDate=DateTime.UtcNow;
                }
            }
            else
            {
                // Add new student
                _dbContext.Students.Add(new Students
                {
                    Id = Guid.NewGuid(),
                    SchoolId = ext.SchoolId,
                    FullName = ext.FullName,
                    Email = ext.Email,
                    Residence = ext.Residence,
                    YearOfStudy = ext.YearOfStudy,
                    AcademicStatus = ext.AcademicStatus,
                    CreatedAt = DateTime.UtcNow,
                    LastSyncDate = DateTime.UtcNow,
                });
                added++;
            }
        }

        await _dbContext.SaveChangesAsync();
        stopwatch.Stop();

        return ApiResponse<SyncResultDto>.SuccessResponse(new SyncResultDto 
        { 
            AddedCount = added, 
            UpdatedCount = updated,
            ExecutionTimeMs = stopwatch.ElapsedMilliseconds
        }, "Daily refresh completed.");
    }
    
    
}