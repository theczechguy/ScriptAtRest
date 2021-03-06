using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using ScriptAtRestServer.Helpers;
using Microsoft.AspNetCore.Authorization;
using ScriptAtRestServer.Services;
using ScriptAtRestServer.Entities;
using ScriptAtRestServer.Models.Scripts;
using ScriptAtRestServer.Models.ScriptTypes;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Http;

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
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterScriptModel Model)
        {
            _logger.LogInformation("Register new script");

            try
            {
                string decodedContent = Base64.DecodeBase64(Model.EncodedContent);
                Script script = _mapper.Map<Script>(Model);
                script.Content = decodedContent;
                Script createdScript = await _scriptService.CreateAsync(script);

                ScriptModel scriptModel = _mapper.Map<ScriptModel>(createdScript);
                _logger.LogInformation("Script registered with id : {scriptId}" , scriptModel.id);
                return Ok(scriptModel);
            }
            catch (AppException ex)
            {
                _logger.LogError(ex , "Failed to register new script");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatal failure");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Fatal internal error. Please contact administrator" });
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatal failure");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Fatal internal error. Please contact administrator" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(int Id) {
            _logger.LogInformation("Get Script with id : {scriptid}", Id);
            try
            {
                var script = await _scriptService.GetByIdAsync(Id);
                var model = _mapper.Map<ScriptModel>(script);
                _logger.LogInformation("Retrieved script : {scriptName}", model.Name);
                return Ok(model);
            }
            catch (AppException ex)
            {
                _logger.LogError(ex, "Failed to get script");
                return BadRequest(new { message = "Failed to get script" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatal failure");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Fatal internal error. Please contact administrator" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int Id) 
        {
            _logger.LogInformation("Delete script with ID : {scriptid}" , Id);

            try
            {
                await _scriptService.DeleteAsync(Id);
                _logger.LogInformation("Script deleted");
                return Ok();
            }
            catch (AppException ex)
            {
                _logger.LogError(ex, "Failed to delete script");
                return BadRequest(new { message = "Failed to delete script" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatal failure");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Fatal internal error. Please contact administrator" });
            }
        }

        [HttpPost("run/{id}")]
        public async Task<IActionResult> ExecuteScriptById([FromBody] ScriptParamArray? Model , int Id)
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatal failure");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Fatal internal error. Please contact administrator" });
            }
        }


        [HttpGet("type")]
        public IActionResult GetAllScriptTypes()
        {
            _logger.LogInformation("Get all script types");
            try
            {
                IEnumerable<ScriptType> scriptTypes = _scriptService.GetAllTypes();
                var model = _mapper.Map<IList<ScriptTypeModel>>(scriptTypes);
                _logger.LogInformation("Script types retrieved : {typesCOunt}", model.Count);
                return Ok(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatal failure whil getting all script types");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Fatal internal error. Please contact administrator" });
            }
        }

        [HttpGet("type/{id}")]
        public async Task<IActionResult> GetScripTypeById(int Id) 
        {
            _logger.LogInformation("Get script type with id : {id}" , Id);
            try
            {
                ScriptType scriptType = await _scriptService.GetTypeByIdAsync(Id);
                var model = _mapper.Map<ScriptTypeModel>(scriptType);
                _logger.LogInformation("Retrieved script type : {scriptName}", model.Name);

                return Ok(model);
            }
            catch (AppException ex)
            {
                _logger.LogWarning(ex, $"Failed find script type with id {Id}");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Fatal failure while getting script type with id {Id}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Fatal internal error. Please contact administrator" });
            }
        }

        [HttpPost("type")]
        public async Task<IActionResult> RegisterScriptType([FromBody] RegisterScriptTypeModel Model)
        {
            _logger.LogInformation("Register new script type");
            try
            {
                ScriptType scriptType = _mapper.Map<ScriptType>(Model);
                scriptType = await _scriptService.CreateTypeAsync(scriptType);

                ScriptTypeModel scriptTypeModel = _mapper.Map<ScriptTypeModel>(scriptType);
                _logger.LogInformation("Registered new script type with id : {id}", scriptTypeModel.Id);
                _logger.LogDebug("New script type : {@scripttype}" , scriptType);

                return Ok(scriptTypeModel);
            }
            catch (AppException ex)
            {
                _logger.LogWarning(ex, "Failed to register script type");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex , "Fatal failure during script type registration");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Fatal internal error. Please contact administrator" });
            }
        }

        [HttpDelete("type/{id}")]
        public async Task<IActionResult> DeleteScriptTypeAsync(int Id)
        {
            _logger.LogInformation("Delete script type with id : {id}" , Id);
            try
            {
                await _scriptService.DeleteTypeAsync(Id);
                _logger.LogInformation("Script type deleted");
                return Ok();
            }
            catch (AppException ex)
            {
                _logger.LogWarning(ex, "Failed to delete script type");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatal failure during scrip type deletion");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Fatal internal error. Please contact administrator" });
            }
        }

        [HttpPut("type/{id}")]
        public async Task<IActionResult> UpdateScriptTypeAsync([FromBody] UpdateScriptTypeModel Model, int Id)
        {
            _logger.LogInformation("Update script type with id : {id}" , Id);
            try
            {
                ScriptType scriptType = _mapper.Map<ScriptType>(Model);
                ScriptType updatedScriptType = await _scriptService.UpdateTypeById(Id, scriptType);
                return Ok(_mapper.Map<ScriptTypeModel>(updatedScriptType));
            }
            catch (AppException ex)
            {
                _logger.LogWarning(ex, "Failed to update script type");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatal failure during scrip type update");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Fatal internal error. Please contact administrator" });
            }
        }
    }
}