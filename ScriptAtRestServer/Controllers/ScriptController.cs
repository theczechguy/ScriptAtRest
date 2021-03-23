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
            Script script = _mapper.Map<Script>(model);
            try
            {
                _scriptService.Create(script);
                return Ok();
            }
            catch (AppException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult GetAll() {
            var scripts = _scriptService.GetAll();
            var model = _mapper.Map<IList<ScriptModel>>(scripts);
            return Ok(model);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int Id) {
            var script = _scriptService.GetById(Id);
            var model = _mapper.Map<ScriptModel>(script);
            return Ok(model);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int Id) {
            _scriptService.Delete(Id);
            return Ok();
        }

        [HttpPost("run/{id}")]
        public async Task<IActionResult> ExecuteScriptById(int id)
        {
            _logger.LogInformation("New request for: Run Script by ID");
            ProcessModel p = await _scriptExecutionService.RunScriptById(id);
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