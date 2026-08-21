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

        // GET: api/Tasks/dashboard
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var dashboard = await _taskService.GetDashboardAsync(User.GetUserId(), User.GetUserRole());
            return Ok(dashboard);
        }

        // GET: api/Tasks/export
        [HttpGet("export")]
        public async Task<IActionResult> ExportTasks()
        {
            var bytes = await _taskService.ExportTasksAsync(User.GetUserId(), User.GetUserRole());
            return File(bytes, "application/json", "tasks.json");
        }

        // POST: api/Tasks/import
        [HttpPost("import")]
        public async Task<IActionResult> ImportTasks([FromBody] List<TaskCreateDto> taskInputs)
        {
            var count = await _taskService.ImportTasksAsync(taskInputs, User.GetUserId());
            return Ok(new { message = $"{count} tasks imported successfully.", importedCount = count });
        }

        // GET: api/Tasks
        [HttpGet]
        public async Task<IActionResult> GetTasks()
        {
            var tasks = await _taskService.GetTasksAsync(User.GetUserId(), User.GetUserRole());
            return Ok(tasks);
        }

        // GET: api/Tasks/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTask(int id)
        {
            var task = await _taskService.GetTaskByIdAsync(id, User.GetUserId(), User.GetUserRole());
            return Ok(task);
        }

        // POST: api/Tasks
        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] TaskCreateDto taskInput)
        {
            var task = await _taskService.CreateTaskAsync(taskInput, User.GetUserId(), User.GetUserRole());
            return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
        }

        // PUT: api/Tasks/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, [FromBody] TaskUpdateDto taskUpdate)
        {
            var task = await _taskService.UpdateTaskAsync(id, taskUpdate, User.GetUserId(), User.GetUserRole());
            return Ok(task);
        }

        // DELETE: api/Tasks/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var message = await _taskService.DeleteTaskAsync(id, User.GetUserId(), User.GetUserRole());
            return Ok(new { message });
        }

        // POST: api/Tasks/5/restore
        [HttpPost("{id}/restore")]
        public async Task<IActionResult> RestoreTask(int id)
        {
            var restoredTask = await _taskService.RestoreTaskAsync(id, User.GetUserId(), User.GetUserRole());
            return Ok(restoredTask);
        }
    }
}