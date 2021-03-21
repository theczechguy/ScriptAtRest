using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ScriptAtRestServer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ScriptAtRestServer.Controllers
{
    [Authorize]
    [ApiController]
    [Route("script")]
    public class ScriptRunnerController : Controller
    {
        private IScriptExecutionService _scriptExecutionService;
        private ILogger<ScriptRunnerController> _logger;

        public ScriptRunnerController(IScriptExecutionService ScriptExecutionService, ILogger<ScriptRunnerController> Logger) {
            _scriptExecutionService = ScriptExecutionService;
            _logger = Logger;
        }

        [HttpGet("run/{Scriptname}")]
        public async Task<IActionResult> ExecuteScript(string Scriptname) {
            _logger.LogInformation("New request for: Run Script");
            ProcessModel p = await _scriptExecutionService.RunScript(
                ScriptEnums.ScriptType.PowerShell,
                Scriptname,
                string.Empty
                );
            _logger.LogInformation("Script exit code : {ExitCode}", p.ExitCode);
            return new ObjectResult(new
            {
                ExitCode = p.ExitCode,
                Output = p.Output,
                ErrorOutput = p.ErrorOutput
            });
        }
    }
}