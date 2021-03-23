﻿using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using ScriptAtRestServer.Helpers;
using Microsoft.AspNetCore.Authorization;
using ScriptAtRestServer.Services;
using ScriptAtRestServer.Entities;
using ScriptAtRestServer.Models.Scripts;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

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

            try
            {
                Script script = _mapper.Map<Script>(model);
                Script createdScript = _scriptService.Create(script);
                _logger.LogInformation("Script registered with id : {scriptId}" , createdScript.Id);
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
            try
            {
                var scripts = _scriptService.GetAll();
                var model = _mapper.Map<IList<ScriptModel>>(scripts);
                _logger.LogInformation("Scripts retrieved : {scriptCount}", model.Count);
                return Ok(model);
            }
            catch (AppException ex)
            {
                _logger.LogError(ex, "Failed to get all scripts");
                return BadRequest(new { message = "Failed to get all scripts" });
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int Id) {
            _logger.LogInformation("Get Script with id : {scriptid}", Id);
            try
            {
                var script = _scriptService.GetById(Id);
                var model = _mapper.Map<ScriptModel>(script);
                _logger.LogInformation("Retrieved script : {scriptName}", model.Name);
                return Ok(model);
            }
            catch (AppException ex)
            {
                _logger.LogError(ex, "Failed to get script");
                return BadRequest(new { message = "Failed to get script" });
            }
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int Id) 
        {
            _logger.LogInformation("Delete script with ID : {scriptid}" , Id);

            try
            {
                _scriptService.Delete(Id);
                return Ok();
            }
            catch (AppException ex)
            {
                _logger.LogError(ex, "Failed to delete script");
                return BadRequest(new { message = "Failed to delete script" });
            }
        }

        [HttpPost("run/{id}")]
        public async Task<IActionResult> ExecuteScriptById(int Id)
        {
            _logger.LogInformation("Run script with ID : {scriptid}" , Id);

            try
            {
                ProcessModel p = await _scriptExecutionService.RunScriptById(Id);
                return Ok(p);
            }
            catch (AppException ex)
            {
                _logger.LogError(ex, "Failed to execute script");
                return BadRequest(new { message = "Failed to execute script" });
            }
        }
    }
}