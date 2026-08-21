using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskManager.API.DTOs;
using TaskManager.API.Services;

namespace TaskManager.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        // GET: api/user/profile
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var profile = await _userService.GetProfileAsync(User.GetUserId());
            return Ok(profile);
        }

        // GET: api/user/all
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        // POST: api/user
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateUser([FromBody] UserCreateAdminDto dto)
        {
            var user = await _userService.CreateUserAsync(dto);
            return CreatedAtAction(nameof(GetAllUsers), new { id = user.Id }, user);
        }

        // PUT: api/user/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserUpdateAdminDto dto)
        {
            var user = await _userService.UpdateUserAsync(id, dto);
            return Ok(user);
        }

        // DELETE: api/user/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var message = await _userService.DeleteUserAsync(id, User.GetUserId());
            return Ok(new { message });
        }
    }
}
