using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace KylesBackendAPI.Controllers
{
    [ApiController]
    [Route("api/{*path}")]
    [EnableCors("AllowAll")]
    public class GlobalCorsController : ControllerBase
    {
        [HttpOptions]
        public IActionResult HandleGlobalOptions()
        {
            Response.Headers.Append("Access-Control-Allow-Origin", "*");
            Response.Headers.Append("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
            Response.Headers.Append("Access-Control-Allow-Headers", "Content-Type, Authorization, Accept");
            return Ok();
        }
    }
}