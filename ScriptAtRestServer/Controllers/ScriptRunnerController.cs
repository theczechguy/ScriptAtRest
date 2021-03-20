using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        public ScriptRunnerController(IScriptExecutionService ScriptExecutionService) {
            _scriptExecutionService = ScriptExecutionService;
        }

        [HttpGet("run/{Scriptname}")]
        public async Task<IActionResult> ExecuteScript(string Scriptname) {
            ProcessModel p = await _scriptExecutionService.RunScript(
                ScriptEnums.ScriptType.PowerShell,
                "Test.ps1",
                string.Empty
                );
            return new ObjectResult(new
            {
                ExitCode = p.ExitCode,
                Output = p.Output,
                ErrorOutput = p.ErrorOutput
            });
        }
    }
}