using Microsoft.EntityFrameworkCore;
using TaskManager.API.Data;
using TaskManager.API.DTOs;
using TaskManager.API.Models;

namespace TaskManager.API.Services
{
    public class TaskService : ITaskService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<TaskService> _logger;

        public TaskService(AppDbContext context, ILogger<TaskService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<TaskResponseDto>> GetTasksAsync(int userId, string userRole)
        {
            _logger.LogInformation("User ID {UserId} (Role: {Role}) is fetching tasks.", userId, userRole);

            var query = _context.Tasks.Include(t => t.AssignedUser).AsQueryable();

            // Admin sees all tasks; regular user sees only their own
            if (userRole != "Admin")
            {
                query = query.Where(t => t.AssignedUserId == userId);
            }

            var tasks = await query.ToListAsync();

            return tasks.Select(MapToResponseDto);
        }

        public async Task<TaskResponseDto> GetTaskByIdAsync(int taskId, int userId, string userRole)
        {
            var query = _context.Tasks.Include(t => t.AssignedUser).AsQueryable();

            if (userRole != "Admin")
            {
                query = query.Where(t => t.AssignedUserId == userId);
            }

            var task = await query.FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null)
            {
                _logger.LogWarning("User ID {UserId} attempted to access Task ID {TaskId} — not found or access denied.", userId, taskId);
                throw new KeyNotFoundException("Task not found.");
            }

            return MapToResponseDto(task);
        }

        public async Task<TaskResponseDto> CreateTaskAsync(TaskCreateDto dto, int userId)
        {
            var task = new TaskItem
            {
                Title = dto.Title,
                Description = dto.Description,
                DueDate = dto.DueDate,
                Priority = dto.Priority,
                Status = dto.Status,
                Category = dto.Category,
                AssignedUserId = userId
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            // Reload with navigation property
            await _context.Entry(task).Reference(t => t.AssignedUser).LoadAsync();

            _logger.LogInformation("User ID {UserId} created Task ID {TaskId}: {Title}", userId, task.Id, task.Title);

            return MapToResponseDto(task);
        }

        public async Task<TaskResponseDto> UpdateTaskAsync(int taskId, TaskUpdateDto dto, int userId, string userRole)
        {
            var query = _context.Tasks.AsQueryable();

            if (userRole != "Admin")
            {
                query = query.Where(t => t.AssignedUserId == userId);
            }

            var task = await query.FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null)
            {
                _logger.LogWarning("User ID {UserId} failed to update Task ID {TaskId} — not found or access denied.", userId, taskId);
                throw new KeyNotFoundException("Task not found or access denied.");
            }

            task.Title = dto.Title;
            task.Description = dto.Description;
            task.DueDate = dto.DueDate;
            task.Priority = dto.Priority;
            task.Status = dto.Status;
            task.Category = dto.Category;

            await _context.SaveChangesAsync();

            // Reload with navigation property
            await _context.Entry(task).Reference(t => t.AssignedUser).LoadAsync();

            _logger.LogInformation("User ID {UserId} updated Task ID {TaskId}", userId, task.Id);

            return MapToResponseDto(task);
        }

        public async Task<string> DeleteTaskAsync(int taskId, int userId, string userRole)
        {
            var query = _context.Tasks.AsQueryable();

            if (userRole != "Admin")
            {
                query = query.Where(t => t.AssignedUserId == userId);
            }

            var task = await query.FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null)
            {
                _logger.LogWarning("User ID {UserId} failed to delete Task ID {TaskId} — not found or access denied.", userId, taskId);
                throw new KeyNotFoundException("Task not found or access denied.");
            }

            // Soft delete
            task.IsDeleted = true;
            await _context.SaveChangesAsync();

            _logger.LogInformation("User ID {UserId} soft-deleted Task ID {TaskId}", userId, task.Id);

            return "Task deleted successfully.";
        }

        public async Task<DashboardDto> GetDashboardAsync(int userId, string userRole)
        {
            _logger.LogInformation("User ID {UserId} (Role: {Role}) is fetching dashboard data.", userId, userRole);

            var query = _context.Tasks.AsQueryable();

            // Admin sees all task counts; regular user sees only their own
            if (userRole != "Admin")
            {
                query = query.Where(t => t.AssignedUserId == userId);
            }

            var pending = await query.CountAsync(t => t.Status == "Pending");
            var inProgress = await query.CountAsync(t => t.Status == "InProgress");
            var completed = await query.CountAsync(t => t.Status == "Completed");

            return new DashboardDto
            {
                PendingCount = pending,
                InProgressCount = inProgress,
                CompletedCount = completed,
                TotalCount = pending + inProgress + completed
            };
        }

        private static TaskResponseDto MapToResponseDto(TaskItem task)
        {
            return new TaskResponseDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                DueDate = task.DueDate,
                Priority = task.Priority,
                Status = task.Status,
                Category = task.Category,
                AssignedUserId = task.AssignedUserId,
                AssignedUserName = task.AssignedUser?.Name ?? string.Empty
            };
        }
    }
}
