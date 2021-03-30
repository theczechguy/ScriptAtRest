using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using ScriptAtRestServer.Helpers;
using Microsoft.AspNetCore.Authorization;
using ScriptAtRestServer.Services;
using ScriptAtRestServer.Entities;
using ScriptAtRestServer.Models.Scripts;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;
using System.Text;
using ScriptAtRestServer.Helpers;

namespace ScriptAtRestServer.Controllers
{
    [Authorize]
    [ApiController]
    [Route("scripts")]
    public class ScriptController : Controller
    {
        private IScriptService _scriptService;
        private IScriptExecutionService _scriptExecutionService;
        private ILogger<ScriptController> _logger;
        private IMapper _mapper;

        public ScriptController(
            IScriptService ScriptService,
            ILogger<ScriptController> Logger,
            IMapper Mapper,
            IScriptExecutionService ScriptExecutionService
        ) {
            _scriptService = ScriptService;
            _scriptExecutionService = ScriptExecutionService;
            _logger = Logger;
            _mapper = Mapper;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterScriptModel Model) {
            _logger.LogInformation("Register new script");

            try
            {
                string decodedContent = Base64.DecodeBase64(Model.EncodedContent);
                Script script = _mapper.Map<Script>(Model);
                script.Content = decodedContent;
                Script createdScript = _scriptService.Create(script);

                ScriptModel scriptModel = _mapper.Map<ScriptModel>(createdScript);
                _logger.LogInformation("Script registered with id : {scriptId}" , scriptModel.id);
                return Ok(scriptModel);
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

        [HttpPost("run/withparams/{id}")]
        public async Task<IActionResult> ExecuteScriptByIdWithParams([FromBody] ScriptParamArray Model , int Id)
        {
            _logger.LogInformation("Run script with ID : {scriptid}" , Id);
            try
            {
                ProcessModel p = await _scriptExecutionService.RunScriptById(Id, Model);
                return Ok(p);
            }
            catch (AppException ex)
            {
                _logger.LogError(ex, "Failed to execute script");
                return BadRequest(new { message = "Failed to execute script" });
            }
        }

        [HttpPost("run/test/{id}")]
        public IActionResult TestParams([FromBody] ScriptParamArray Model , int Id) {
            _logger.LogInformation("Run TestParams");
            _logger.LogInformation("Script ID : {id}" , Id);
            StringBuilder stringBuilder = new StringBuilder();

            if (Model.Parameters.Count > 0)
            {
                _logger.LogInformation("Count of received parameters : {paramcount}", Model.Parameters.Count);
                foreach (ScriptParamModel paramModel in Model.Parameters)
                {
                    string combined = string.Format(" -{0} {1}", paramModel.ParameterName,paramModel.ParameterValue);
                    stringBuilder.Append(combined);
                }
                _logger.LogInformation("Parameter string : {params}" , stringBuilder.ToString());
            }
            else
            {
                _logger.LogInformation("No parameters received");
            }
            return Ok();
        }
    }
}