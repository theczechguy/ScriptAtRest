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
using ScriptAtRestServer.Models.Users;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace ScriptAtRestServer.Controllers
{
    [Authorize]
    [ApiController]
    [Route("users")]
    public class UsersController : ControllerBase
    {
        private IUserService _userService;
        private IMapper _mapper;
        private ILogger<UsersController> _logger;

        public UsersController(
            IUserService userService,
            IMapper mapper,
            ILogger<UsersController> Logger)
        {
            _userService = userService;
            _mapper = mapper;
            _logger = Logger;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public IActionResult Register([FromBody]RegisterModel model)
        {
            _logger.LogInformation("Register new user with username : {username}" , model.Username);
            // map model to entity
            var user = _mapper.Map<User>(model);

            try
            {
                // create user
                User newUser = _userService.Create(user, model.Password);
                UserModel newModel = _mapper.Map<UserModel>(newUser);
                _logger.LogInformation("User registered with id : {userid}", newModel.Id);
                return CreatedAtAction(nameof(GetUserByIdAsync), new { newModel.Id }, newModel);
            }
            catch (AppException ex)
            {
                // return error message if there was an exception
                _logger.LogError(ex, "Failed to register new user");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatal failure while registering new user");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Fatal internal error. Please contact administrator" });
            }
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            _logger.LogInformation("Get all users");
            try
            {
                var users = _userService.GetAll();
                var model = _mapper.Map<IList<UserModel>>(users);
                return Ok(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatal failure while getting all users");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Fatal internal error. Please contact administrator" });
            }
        }

        public async Task<IActionResult> GetUserByIdAsync(int Id) {
            _logger.LogInformation("Get user with id : {id}", Id);
            try
            {
                User user = await _userService.GetByIdAsync(Id);
                return Ok(user);
            }
            catch (AppException ex)
            {
                // return error message if there was an exception
                _logger.LogError(ex, "Failed to find user");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatal failure");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Fatal internal error. Please contact administrator" });
            }
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int Id)
        {
            _logger.LogInformation("Delete user with id : {userid}" , Id);
            try
            {
                _userService.Delete(Id);
                _logger.LogInformation("User deleted");
                return Ok();
            }
            catch (AppException ex)
            {
                _logger.LogError(ex, "Failed to delete user");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatal failure while deleting user");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Fatal internal error. Please contact administrator" });
            }
        }
    }
}