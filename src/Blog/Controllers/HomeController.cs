//using Blog.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Controllers
{
    [ApiController] 
    [Route("")]
    public class HomeController : ControllerBase
    {
        [HttpGet("")]
        //[ApiKey]
        public IActionResult Get(
            [FromServices]IConfiguration config)// ter acesso aos appsettings..
        { try
            {
                var env = config.GetValue<string>(("Env"));
                return Ok(new {
                    Environment = env,
                    message = "API Online"
                }) ;
            }
         catch
            {
                return StatusCode(500, "05XE02 - Falha interna no servidor");
            }
        }                         
    }
}
