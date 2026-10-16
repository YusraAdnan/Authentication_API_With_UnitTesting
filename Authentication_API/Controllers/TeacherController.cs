using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Authentication_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TeacherController : ControllerBase
    {
        [Authorize(Roles = "Teacher")]
        [HttpGet("teacher-only")]
        public IActionResult TeacherOnly()
        {
            return Ok("Hello Teacher! You are authorized.");
        }
    }
}
