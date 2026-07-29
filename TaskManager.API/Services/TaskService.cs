using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TaskManager.API.Data;
using TaskManager.API.DTOs;
using TaskManager.API.Hubs;
using TaskManager.API.Models;

namespace TaskManager.API.Services
{
    public class TaskService : ITaskService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<TaskService> _logger;
        private readonly IHubContext<TaskHub>? _hubContext;

        public TaskService(AppDbContext context, ILogger<TaskService> logger, IHubContext<TaskHub>? hubContext = null)
        {
            _context = context;
            _logger = logger;
            _hubContext = hubContext;
        }

        public async Task<IEnumerable<TaskResponseDto>> GetTasksAsync(int userId, string userRole)
        {
            _logger.LogInformation("User ID {UserId} (Role: {Role}) is fetching tasks.", userId, userRole);

            var query = _context.Tasks.Include(t => t.AssignedUser).Include(t => t.SubTasks).AsQueryable();

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
            var query = _context.Tasks.Include(t => t.AssignedUser).Include(t => t.SubTasks).AsQueryable();

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
                AssignedUserId = userId,
                SubTasks = dto.SubTasks.Select(st => new SubTask
                {
                    Title = st.Title,
                    IsCompleted = st.IsCompleted
                }).ToList()
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            // Reload with navigation properties
            await _context.Entry(task).Reference(t => t.AssignedUser).LoadAsync();
            await _context.Entry(task).Collection(t => t.SubTasks).LoadAsync();

            _logger.LogInformation("User ID {UserId} created Task ID {TaskId}: {Title}", userId, task.Id, task.Title);

            var responseDto = MapToResponseDto(task);
            if (_hubContext != null)
            {
                await _hubContext.Clients.All.SendAsync("TaskCreated", responseDto);
            }
            return responseDto;
        }

        public async Task<TaskResponseDto> UpdateTaskAsync(int taskId, TaskUpdateDto dto, int userId, string userRole)
        {
            var query = _context.Tasks.Include(t => t.SubTasks).AsQueryable();

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

            // Replace existing subtasks with new ones
            _context.SubTasks.RemoveRange(task.SubTasks);
            task.SubTasks = dto.SubTasks.Select(st => new SubTask
            {
                Title = st.Title,
                IsCompleted = st.IsCompleted
            }).ToList();

            await _context.SaveChangesAsync();

            // Reload with navigation property
            await _context.Entry(task).Reference(t => t.AssignedUser).LoadAsync();

            _logger.LogInformation("User ID {UserId} updated Task ID {TaskId}", userId, task.Id);

            var updatedDto = MapToResponseDto(task);
            if (_hubContext != null)
            {
                await _hubContext.Clients.All.SendAsync("TaskUpdated", updatedDto);
            }
            return updatedDto;
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

            if (_hubContext != null)
            {
                await _hubContext.Clients.All.SendAsync("TaskDeleted", taskId);
            }

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

        public async Task<byte[]> ExportTasksAsync(int userId, string userRole)
        {
            var tasks = await GetTasksAsync(userId, userRole);
            var jsonBytes = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(tasks, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            });
            _logger.LogInformation("User ID {UserId} exported {Count} tasks.", userId, tasks.Count());
            return jsonBytes;
        }

        public async Task<int> ImportTasksAsync(IEnumerable<TaskCreateDto> dtos, int userId)
        {
            if (dtos == null || !dtos.Any())
            {
                return 0;
            }

            int count = 0;
            foreach (var dto in dtos)
            {
                if (string.IsNullOrWhiteSpace(dto.Title)) continue;

                var task = new TaskItem
                {
                    Title = dto.Title,
                    Description = dto.Description ?? string.Empty,
                    DueDate = dto.DueDate,
                    Priority = string.IsNullOrWhiteSpace(dto.Priority) ? "Medium" : dto.Priority,
                    Status = string.IsNullOrWhiteSpace(dto.Status) ? "Pending" : dto.Status,
                    Category = dto.Category ?? string.Empty,
                    AssignedUserId = userId
                };
                _context.Tasks.Add(task);
                count++;
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("User ID {UserId} imported {Count} tasks.", userId, count);
            return count;
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
                AssignedUserName = task.AssignedUser?.Name ?? string.Empty,
                SubTasks = task.SubTasks?.Select(st => new SubTaskDto
                {
                    Id = st.Id,
                    Title = st.Title,
                    IsCompleted = st.IsCompleted
                }).ToList() ?? new List<SubTaskDto>()
            };
        }
    }
}
