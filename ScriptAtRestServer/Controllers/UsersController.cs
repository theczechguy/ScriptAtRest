﻿using System;
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
                _userService.Create(user, model.Password);
                _logger.LogInformation("User registered");
                return Ok(new { message = "User registered"});
            }
            catch (AppException ex)
            {
                // return error message if there was an exception
                _logger.LogError(ex , "Failed to register new user");
                return BadRequest(new { message = ex.Message });
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
            catch (AppException ex)
            {
                _logger.LogError(ex , "Failed to get users");
                return BadRequest(new { message = ex.Message });
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
        }
    }
}