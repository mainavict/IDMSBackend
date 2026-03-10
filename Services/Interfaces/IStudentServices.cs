using IDMSBackend.DTOs;
using IDMSBackend.Models;
using IDMSBackend.Wrappers;


namespace IDMSBackend.Services.Interfaces;


public interface IStudentServices
{
    Task<ApiResponse<List<StudentResponseDtos>>> GetAllStudentsAsync();
    Task<ApiResponse<StudentResponseDtos>> GetStudentByIdAsync(String id);

    Task<ApiResponse<SyncResultDto>> SyncStudentsAsync(List<StudentSyncDto> externalStudents);
}