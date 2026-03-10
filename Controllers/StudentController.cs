using IDMSBackend.Data;
using IDMSBackend.DTOs;
using IDMSBackend.Models;
using IDMSBackend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using IDMSBackend.Services.Implementations;

namespace IDMSBackend.Controllers;
[ApiController]
[Route("api/[controller]")]
public class StudentController : ControllerBase
{
    private readonly IStudentServices _studentService;
    
    public StudentController(IStudentServices studentService)
    {
        _studentService = studentService;
    }


    [HttpGet]
    public async Task<IActionResult> GetAllStudents()
    {
        try
        {
            var response = await _studentService.GetAllStudentsAsync();
            return StatusCode(response.StatusCode, response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Success = false, Message = "An error occurred while retrieving students.", Details = ex.Message });
        }
    }

    [HttpGet("{studentId}")]
    public async Task<IActionResult> GetStudentById(string studentId)
    {
        try
        {
            var response = await _studentService.GetStudentByIdAsync(studentId);
            return StatusCode(response.StatusCode, response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Success = false, Message = "An error occurred while retrieving the student.", Details = ex.Message });
        }
    }
    
    [HttpPost("sync")]
        public async Task<IActionResult> SyncStudents([FromBody] List<StudentSyncDto> externalData)
        {
            try
            {
                if (externalData == null || !externalData.Any())
                {
                    return BadRequest(new { Success = false, Message = "No student data provided for synchronization." });
                }
                var result = await _studentService.SyncStudentsAsync(externalData);
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = "An error occurred while synchronizing students.", Details = ex.Message });
            }
        }
    
}