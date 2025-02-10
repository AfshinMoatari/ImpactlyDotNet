using API.Constants;
using API.Models;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class IndexController : ControllerBase
    {
        [HttpGet]
        public IActionResult Index()
        {
            return Ok("OK");
        }

        [HttpGet("version")]
        public ActionResult<string> Version()
        {
            return Ok($"version {EnvironmentMode.Version} {EnvironmentMode.Environment}");
        }

        [HttpGet("health")]
        public ActionResult<string> Health()
        {
            return Ok();
        }

        [HttpGet("error/{status}")]
        public ActionResult StatusCodeError([FromRoute] int status)
        {
            return ErrorResponse.Status(status);
        }
    }
}