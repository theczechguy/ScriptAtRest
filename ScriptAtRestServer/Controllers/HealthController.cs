using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ScriptAtRestServer.Controllers
{
    [ApiController]
    [Route("Health")]
    public class HealthController : ControllerBase
    {
        [AllowAnonymous]
        [HttpGet("Alive")]
        public IActionResult Alive()
        {
            return Ok(new
            {
                Answer = "I'm Alive",
                AnswerDate = DateTime.Now.ToString()
            });
        }
    }
}