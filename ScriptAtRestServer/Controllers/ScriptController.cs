using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using ScriptAtRestServer.Helpers;
using Microsoft.Extensions.Options;
using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using ScriptAtRestServer.Services;
using ScriptAtRestServer.Entities;
using ScriptAtRestServer.Models.Scripts;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using ScriptAtRestServer.Enums;

namespace ScriptAtRestServer.Controllers
{
    [Authorize]
    [ApiController]
    [Route("scripts")]
    public class ScriptController : Controller
    {
        private IScriptService _scriptService;
        private ILogger<ScriptController> _logger;
        private IMapper _mapper;
        private IScriptExecutionService _scriptExecutionService;

        public ScriptController(
            IScriptService ScriptService,
            ILogger<ScriptController> Logger,
            IMapper Mapper,
            IScriptExecutionService ScriptExecutionService
        ) {
            _scriptService = ScriptService;
            _logger = Logger;
            _mapper = Mapper;
            _scriptExecutionService = ScriptExecutionService;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterScriptModel model) {
            _logger.LogInformation("Register new script");
            _logger.LogDebug("{@registerScriptModel}" , model);
            Script script = _mapper.Map<Script>(model);
            try
            {
                Script createdScript = _scriptService.Create(script);
                _logger.LogInformation("Script registered with id : @{scriptId}" , createdScript.Id);
                return Ok();
            }
            catch (AppException ex)
            {
                _logger.LogError(ex , "Failed to register new script");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult GetAll() {
            _logger.LogInformation("Get all scripts");
            var scripts = _scriptService.GetAll();
            var model = _mapper.Map<IList<ScriptModel>>(scripts);
            _logger.LogDebug("Scripts retrieved : @{scriptCount}" , model.Count);
            return Ok(model);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int Id) {
            _logger.LogInformation("Get Script with id : @{scriptid}" , Id);
            var script = _scriptService.GetById(Id);
            var model = _mapper.Map<ScriptModel>(script);
            _logger.LogDebug("Retrieved script : @{script}" , script);
            return Ok(model);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int Id) {
            _logger.LogInformation("Delete script with ID : @{scriptid}" , Id);
            _scriptService.Delete(Id);
            return Ok();
        }

        [HttpPost("run/{id}")]
        public async Task<IActionResult> ExecuteScriptById(int Id)
        {
            _logger.LogInformation("Run script with ID : @{scriptid}" , Id);
            ProcessModel p = await _scriptExecutionService.RunScriptById(Id);
            _logger.LogInformation("Script exit code : {ExitCode}", p.ExitCode);
            _logger.LogDebug("Script result : @{processModel}" , p);

            return Ok(p);
        }
    }
}