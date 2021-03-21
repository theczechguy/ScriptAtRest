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

namespace ScriptAtRestServer.Controllers
{
    [Authorize]
    [ApiController]
    [Route("script")]
    public class ScriptController : Controller
    {
        private IScriptService _scriptService;
        private ILogger<ScriptController> _logger;
        private IMapper _mapper;

        public ScriptController(
            IScriptService ScriptService,
            ILogger<ScriptController> Logger,
            IMapper Mapper
        ) {
            _scriptService = ScriptService;
            _logger = Logger;
            _mapper = Mapper;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] ScriptModel model) {
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
    }
}