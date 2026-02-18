using IDMSBackend.DTOs;
using IDMSBackend.Services.Interfaces;
using  IDMSBackend.Models;
using  IDMSBackend.Wrappers;

namespace IDMSBackend.Services.Interfaces;

public interface IStudentCards
{
    Task <ApiResponse<StudentCardsDto>>CreateStudentCardAsync(CreateStudentCardsDto createDto);
    Task <ApiResponse<StudentCardsDto>> GetStudentCardBySchoolIdAsync(string schoolId);
   // Task <bool> UpdateStudentCardAsync(UpdateStudentCardsDto updateDto);

}
