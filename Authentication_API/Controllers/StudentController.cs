using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Authentication_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentController : ControllerBase
    {
        //This is the door at which the Autharization Guards
        [Authorize(Roles = "Student")]
        [HttpGet("student-only")]
        public IActionResult StudentOnly()
        {
            return Ok("Hello Student! You are authorized.");
        }

        //Create teacher controller
        //Create a controller that does not need any authorization
        //A controller than needs authorization but no role
    }
}
