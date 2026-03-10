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
        _logger.LogInformation("Retrieving all students from the database");
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

    // 1. Optimized Fetch: Only pull the columns needed for comparison
    // This keeps the memory footprint small even if you have thousands of records.
    var localStudents = await _dbContext.Students
        .Select(s => new { 
            s.Id, 
            s.SchoolId, 
            s.FullName, 
            s.Email, 
            s.Residence, 
            s.YearOfStudy, 
            s.AcademicStatus 
        })
        .ToDictionaryAsync(s => s.SchoolId);

    int added = 0;
    int updated = 0;
    int batchSize = 50; 
    int currentCount = 0;

    foreach (var ext in externalStudents)
    {
        currentCount++;

        if (localStudents.TryGetValue(ext.SchoolId, out var existingInfo))
        {
            // Check if any field has actually changed
            bool hasChanged = existingInfo.FullName != ext.FullName ||
                              existingInfo.Email != ext.Email ||
                              existingInfo.Residence != ext.Residence ||
                              existingInfo.YearOfStudy != ext.YearOfStudy ||
                              existingInfo.AcademicStatus != ext.AcademicStatus;

            if (hasChanged)
            {
                // We fetch the actual entity to update it (EF Core needs the real object to track changes)
                var studentToUpdate = new Students { Id = existingInfo.Id };
                _dbContext.Students.Attach(studentToUpdate);

                studentToUpdate.FullName = ext.FullName;
                studentToUpdate.Email = ext.Email;
                studentToUpdate.Residence = ext.Residence;
                studentToUpdate.YearOfStudy = ext.YearOfStudy;
                studentToUpdate.AcademicStatus = ext.AcademicStatus;
                studentToUpdate.UpdatedAt = DateTime.UtcNow;
                studentToUpdate.LastSyncDate = DateTime.UtcNow;
                
                updated++;
            }
        }
        else
        {
            // 2. Add new student
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

        // 3. BATCH SAVE: This prevents the "Failed executing DbCommand" error
        if (currentCount % batchSize == 0)
        {
            await _dbContext.SaveChangesAsync();
            // Optional: Clears the tracker to keep memory usage low during large syncs
            _dbContext.ChangeTracker.Clear(); 
        }
    }

    // 4. Final Save for any remaining records (e.g., the last 4 students)
    await _dbContext.SaveChangesAsync();
    
    stopwatch.Stop();

    return ApiResponse<SyncResultDto>.SuccessResponse(new SyncResultDto 
    { 
        AddedCount = added, 
        UpdatedCount = updated,
        ExecutionTimeMs = stopwatch.ElapsedMilliseconds
    }, $"Sync completed: {added} added, {updated} updated.");
}

}