using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskManager.API.Data;
using TaskManager.API.Models;

namespace TaskManager.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<TasksController> _logger;

        public TasksController(AppDbContext context, ILogger<TasksController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Helper method to extract the logged-in user's ID from the JWT token
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out int userId) ? userId : 0;
        }

        // GET: api/Tasks
        [HttpGet]
        public async Task<IActionResult> GetTasks()
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("User ID {UserId} is fetching their task list.", userId);

            // The Global Query Filter in DbContext automatically hides IsDeleted == true
            var tasks = await _context.Tasks
                .Where(t => t.AssignedUserId == userId)
                .ToListAsync();

            return Ok(tasks);
        }

        // GET: api/Tasks/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTask(int id)
        {
            var userId = GetCurrentUserId();
            var task = await _context.Tasks
                .FirstOrDefaultAsync(t => t.Id == id && t.AssignedUserId == userId);

            if (task == null)
            {
                _logger.LogWarning("User ID {UserId} attempted to access Task ID {TaskId} which does not exist or belongs to someone else.", userId, id);
                return NotFound("Task not found.");
            }

            return Ok(task);
        }

        // POST: api/Tasks
        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] TaskItem taskInput)
        {
            var userId = GetCurrentUserId();
            
            var task = new TaskItem
            {
                Title = taskInput.Title,
                Description = taskInput.Description,
                DueDate = taskInput.DueDate,
                Priority = taskInput.Priority,
                Status = taskInput.Status,
                Category = taskInput.Category,
                AssignedUserId = userId
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User ID {UserId} successfully created Task ID {TaskId}", userId, task.Id);

            return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
        }

        // PUT: api/Tasks/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, [FromBody] TaskItem taskUpdate)
        {
            var userId = GetCurrentUserId();
            var task = await _context.Tasks
                .FirstOrDefaultAsync(t => t.Id == id && t.AssignedUserId == userId);

            if (task == null)
            {
                _logger.LogWarning("User ID {UserId} failed to update Task ID {TaskId} (Not Found or Access Denied).", userId, id);
                return NotFound("Task not found or access denied.");
            }

            task.Title = taskUpdate.Title;
            task.Description = taskUpdate.Description;
            task.DueDate = taskUpdate.DueDate;
            task.Priority = taskUpdate.Priority;
            task.Status = taskUpdate.Status;
            task.Category = taskUpdate.Category;

            await _context.SaveChangesAsync();

            _logger.LogInformation("User ID {UserId} successfully updated Task ID {TaskId}", userId, task.Id);

            return Ok(task);
        }

        // DELETE: api/Tasks/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var userId = GetCurrentUserId();
            var task = await _context.Tasks
                .FirstOrDefaultAsync(t => t.Id == id && t.AssignedUserId == userId);

            if (task == null)
            {
                _logger.LogWarning("User ID {UserId} failed to delete Task ID {TaskId} (Not Found or Access Denied).", userId, id);
                return NotFound("Task not found or access denied.");
            }

            // Virtual/Soft Delete implementation
            task.IsDeleted = true;
            
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("User ID {UserId} successfully soft-deleted Task ID {TaskId}", userId, task.Id);

            return Ok(new { message = "Task deleted successfully." });
        }
    }
}