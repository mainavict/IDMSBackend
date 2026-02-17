using Microsoft.AspNetCore.Mvc;

namespace IDMSBackend.Controllers;
[ApiController]
[Route("api/[controller]")]
public class HealthController: ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Redirect("/health");   
    
}