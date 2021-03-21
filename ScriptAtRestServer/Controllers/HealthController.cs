using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
        private ILogger<HealthController> _logger;

        public HealthController(ILogger<HealthController> Logger) {
            _logger = Logger;
        }

        [AllowAnonymous]
        [HttpGet("Alive")]
        public IActionResult Alive()
        {
            _logger.LogInformation("New request for: Healthcheck");
            return Ok(new
            {
                Answer = "I'm Alive",
                AnswerDate = DateTime.Now.ToString()
            });
        }
    }
}