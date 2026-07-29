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
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _taskService;

        public TasksController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        // Helper method to extract the logged-in user's ID from the JWT token
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out int userId) ? userId : 0;
        }

        private string GetCurrentUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value ?? "User";
        }

        // GET: api/Tasks/dashboard
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();
            var dashboard = await _taskService.GetDashboardAsync(userId, userRole);
            return Ok(dashboard);
        }

        // GET: api/Tasks/export
        [HttpGet("export")]
        public async Task<IActionResult> ExportTasks()
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();
            var bytes = await _taskService.ExportTasksAsync(userId, userRole);
            return File(bytes, "application/json", "tasks.json");
        }

        // POST: api/Tasks/import
        [HttpPost("import")]
        public async Task<IActionResult> ImportTasks([FromBody] List<TaskCreateDto> taskInputs)
        {
            var userId = GetCurrentUserId();
            var count = await _taskService.ImportTasksAsync(taskInputs, userId);
            return Ok(new { message = $"{count} tasks imported successfully.", importedCount = count });
        }

        // GET: api/Tasks
        [HttpGet]
        public async Task<IActionResult> GetTasks()
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();
            var tasks = await _taskService.GetTasksAsync(userId, userRole);
            return Ok(tasks);
        }

        // GET: api/Tasks/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTask(int id)
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();
            var task = await _taskService.GetTaskByIdAsync(id, userId, userRole);
            return Ok(task);
        }

        // POST: api/Tasks
        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] TaskCreateDto taskInput)
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();
            var task = await _taskService.CreateTaskAsync(taskInput, userId, userRole);
            return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
        }

        // PUT: api/Tasks/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, [FromBody] TaskUpdateDto taskUpdate)
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();
            var task = await _taskService.UpdateTaskAsync(id, taskUpdate, userId, userRole);
            return Ok(task);
        }

        // DELETE: api/Tasks/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();
            var message = await _taskService.DeleteTaskAsync(id, userId, userRole);
            return Ok(new { message });
        }
    }
}