namespace IDMSBackend.DTOs;

public class StudentResponseDtos
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string SchoolId { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public  string YearOfStudy { get; set; }
    public string Residence { get; set; }
    public string AcademicStatus { get; set; }
    public DateTime LastSyncDate { get; set; }
    
}

public class CreateStudentDtos
{
    public string Name { get; set; }
    public string SchoolId { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string YearOfStudy { get; set; }
    public string Residence { get; set; }
    public string AcademicStatus { get; set; }
}

public class StudentSyncDto
{
    public string SchoolId { get; set; } = string.Empty;
    
    public string FullName { get; set; } = string.Empty;
    
    public string Email { get; set; } = string.Empty;
    
    public string YearOfStudy { get; set; } = string.Empty;
    
    public string Residence { get; set; } = string.Empty;
    public string AcademicStatus { get; set; } = string.Empty;
}

public class StudentBulkSyncRequest

{

    public DateTime SyncTimestamp { get; set; } = DateTime.UtcNow;

    public List<StudentSyncDto> Students { get; set; } = new();

}

public class SyncResultDto
{
    // How many brand new students were inserted into your DB
    public int AddedCount { get; set; }

    // How many existing students had their data (like Hall of Residence) changed
    public int UpdatedCount { get; set; }

    // How many students stayed exactly the same (no changes needed)
    public int UnchangedCount { get; set; }

    // How long the entire database operation took in milliseconds
    public long ExecutionTimeMs { get; set; }

    // The timestamp of when this sync finished
    public DateTime SyncTimestamp { get; set; } = DateTime.UtcNow;
}