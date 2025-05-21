using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace KylesBackendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowAll")]
    public class CorsPreflightController : ControllerBase
    {
        [HttpOptions]
        public IActionResult HandleOptionsRequest()
        {
            Response.Headers.Append("Access-Control-Allow-Origin", "*");
            Response.Headers.Append("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
            Response.Headers.Append("Access-Control-Allow-Headers", "Content-Type, Authorization, Accept");
            return Ok();
        }
    }
}