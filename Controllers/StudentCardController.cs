using IDMSBackend.DTOs;
using IDMSBackend.Services.Interfaces;
using IDMSBackend.Models;
using Microsoft.AspNetCore.Mvc;
using IDMSBackend.Services.Implementations;

namespace IDMSBackend.Controllers;
[ApiController]
[Route("api/[controller]")]
public class StudentCardController : ControllerBase
{
    private readonly IStudentCards _studentCardService;

    public StudentCardController(IStudentCards studentCardService)
    {
        _studentCardService = studentCardService;
    }
    
    
    [HttpPost]
    public async Task<IActionResult> CreateStudentCard( CreateStudentCardsDto createDto)
    {
        try
        {
            var response = await _studentCardService.CreateStudentCardAsync(createDto);
            return StatusCode(response.StatusCode, response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Success = false, Message = "An error occurred while creating the student card.", Details = ex.Message });
        }
    }

    [HttpGet("{schoolId}")]
    public async Task<IActionResult> GetStudentCardBySchoolId(string schoolId)
    {
        try
        {
            var response = await _studentCardService.GetStudentCardBySchoolIdAsync(schoolId);
            return StatusCode(response.StatusCode, response);
        }
        catch (Exception ex)
        {
            return StatusCode(500,
                new
                {
                    Success = false, Message = "An error occurred while retrieving the student card.",
                    Details = ex.Message
                });
        }
    }

}