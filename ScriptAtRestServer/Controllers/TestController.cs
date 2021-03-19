using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ScriptAtRestServer.Controllers
{
    [Authorize]
    [ApiController]
    [Route("test")]
    public class TestController : Controller
    {
        [HttpGet("authenticate")]
        public IActionResult Test() {
            return Ok(new { 
                ExitCode = 0,
                Output = "Ahoj"
            });
        }
    }
}
