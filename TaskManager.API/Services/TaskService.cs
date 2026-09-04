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

            var query = _context.Tasks.AsNoTracking().Include(t => t.AssignedUser).Include(t => t.SubTasks).AsQueryable();

            if (userRole == "Admin")
            {
                query = query.IgnoreQueryFilters();
            }
            else
            {
                query = query.Where(t => t.AssignedUserId == userId);
            }

            var tasks = await query.ToListAsync();

            return tasks.Select(MapToResponseDto);
        }

        public async Task<TaskResponseDto> GetTaskByIdAsync(int taskId, int userId, string userRole)
        {
            var query = _context.Tasks.AsNoTracking().Include(t => t.AssignedUser).Include(t => t.SubTasks).AsQueryable();

            if (userRole == "Admin")
            {
                query = query.IgnoreQueryFilters();
            }
            else
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

        public async Task<TaskResponseDto> CreateTaskAsync(TaskCreateDto dto, int userId, string userRole = "User")
        {
            int targetUserId = userId;
            if (userRole == "Admin" && dto.AssignedUserId.HasValue && dto.AssignedUserId.Value > 0)
            {
                var userExists = await _context.Users.AnyAsync(u => u.Id == dto.AssignedUserId.Value);
                if (!userExists)
                {
                    throw new KeyNotFoundException($"Assigned user ID {dto.AssignedUserId.Value} not found.");
                }
                targetUserId = dto.AssignedUserId.Value;
            }

            bool isAdminAssigned = (userRole == "Admin" && dto.AssignedUserId.HasValue && dto.AssignedUserId.Value > 0 && dto.AssignedUserId.Value != userId);

            var task = new TaskItem
            {
                Title = dto.Title,
                Description = dto.Description,
                DueDate = dto.DueDate,
                Priority = dto.Priority,
                Status = dto.Status,
                Category = dto.Category,
                AssignedUserId = targetUserId,
                IsAdminAssigned = isAdminAssigned,
                SubTasks = (dto.SubTasks ?? Enumerable.Empty<SubTaskDto>()).Select(st => new SubTask
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

            _logger.LogInformation("User ID {UserId} created Task ID {TaskId}: {Title} (Assigned to User ID {AssignedUserId})", userId, task.Id, task.Title, task.AssignedUserId);

            var responseDto = MapToResponseDto(task);
            if (_hubContext != null)
            {
                await _hubContext.Clients.Group($"User_{task.AssignedUserId}").SendAsync("TaskCreated", responseDto);
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

            if (userRole == "Admin" && dto.AssignedUserId.HasValue && dto.AssignedUserId.Value > 0)
            {
                var userExists = await _context.Users.AnyAsync(u => u.Id == dto.AssignedUserId.Value);
                if (!userExists)
                {
                    throw new KeyNotFoundException($"Assigned user ID {dto.AssignedUserId.Value} not found.");
                }
                task.AssignedUserId = dto.AssignedUserId.Value;
                task.IsAdminAssigned = dto.AssignedUserId.Value != userId;
            }

            // Replace existing subtasks with new ones
            _context.SubTasks.RemoveRange(task.SubTasks);
            task.SubTasks = (dto.SubTasks ?? Enumerable.Empty<SubTaskDto>()).Select(st => new SubTask
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
                await _hubContext.Clients.Group($"User_{task.AssignedUserId}").SendAsync("TaskUpdated", updatedDto);
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

            if (userRole != "Admin" && task.IsAdminAssigned)
            {
                _logger.LogWarning("User ID {UserId} attempted to delete Admin-assigned Task ID {TaskId}.", userId, taskId);
                throw new InvalidOperationException("Tasks assigned by an Admin cannot be deleted by a regular user.");
            }

            // Soft delete
            task.IsDeleted = true;
            await _context.SaveChangesAsync();

            _logger.LogInformation("User ID {UserId} soft-deleted Task ID {TaskId}", userId, task.Id);

            if (_hubContext != null)
            {
                await _hubContext.Clients.Group($"User_{task.AssignedUserId}").SendAsync("TaskDeleted", taskId);
            }

            return "Task deleted successfully.";
        }

        public async Task<TaskResponseDto> RestoreTaskAsync(int taskId, int userId, string userRole)
        {
            if (userRole != "Admin")
            {
                throw new InvalidOperationException("Only Admin users can restore soft-deleted tasks.");
            }

            var task = await _context.Tasks.IgnoreQueryFilters()
                .Include(t => t.AssignedUser)
                .Include(t => t.SubTasks)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null)
            {
                throw new KeyNotFoundException("Task not found.");
            }

            task.IsDeleted = false;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Admin User ID {UserId} restored Task ID {TaskId}", userId, task.Id);

            var restoredDto = MapToResponseDto(task);
            if (_hubContext != null)
            {
                await _hubContext.Clients.Group($"User_{task.AssignedUserId}").SendAsync("TaskUpdated", restoredDto);
            }

            return restoredDto;
        }

        public async Task<DashboardDto> GetDashboardAsync(int userId, string userRole)
        {
            _logger.LogInformation("User ID {UserId} (Role: {Role}) is fetching dashboard data.", userId, userRole);

            var query = _context.Tasks.AsNoTracking().AsQueryable();

            if (userRole != "Admin")
            {
                query = query.Where(t => t.AssignedUserId == userId);
            }

            var tasks = await query.ToListAsync();

            var totalTasks = tasks.Count;
            var pendingTasks = tasks.Count(t => t.Status == "Pending");
            var inProgressTasks = tasks.Count(t => t.Status == "InProgress");
            var completedTasks = tasks.Count(t => t.Status == "Completed");

            var highPriorityTasks = tasks.Count(t => t.Priority == "High");
            var mediumPriorityTasks = tasks.Count(t => t.Priority == "Medium");
            var lowPriorityTasks = tasks.Count(t => t.Priority == "Low");

            var recentTasks = tasks
                .OrderByDescending(t => t.Id)
                .Take(5)
                .Select(MapToResponseDto)
                .ToList();

            return new DashboardDto
            {
                TotalTasks = totalTasks,
                PendingTasks = pendingTasks,
                InProgressTasks = inProgressTasks,
                CompletedTasks = completedTasks,
                HighPriorityTasks = highPriorityTasks,
                MediumPriorityTasks = mediumPriorityTasks,
                LowPriorityTasks = lowPriorityTasks,
                RecentTasks = recentTasks
            };
        }

        public async Task<byte[]> ExportTasksAsync(int userId, string userRole)
        {
            var tasks = await GetTasksAsync(userId, userRole);
            var json = System.Text.Json.JsonSerializer.Serialize(tasks, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            });

            return System.Text.Encoding.UTF8.GetBytes(json);
        }

        public async Task<int> ImportTasksAsync(IEnumerable<TaskCreateDto> dtos, int userId)
        {
            if (dtos == null)
            {
                return 0;
            }

            var dtoList = dtos as IReadOnlyCollection<TaskCreateDto> ?? dtos.ToList();
            if (dtoList.Count == 0)
            {
                return 0;
            }

            var validPriorities = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "High", "Medium", "Low" };
            var validStatuses = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Pending", "InProgress", "Completed" };

            var validTasks = dtoList
                .Where(dto => !string.IsNullOrWhiteSpace(dto.Title))
                .Select(dto => new TaskItem
                {
                    Title = dto.Title,
                    Description = dto.Description ?? string.Empty,
                    DueDate = dto.DueDate,
                    Priority = (!string.IsNullOrWhiteSpace(dto.Priority) && validPriorities.Contains(dto.Priority)) ? dto.Priority : "Medium",
                    Status = (!string.IsNullOrWhiteSpace(dto.Status) && validStatuses.Contains(dto.Status)) ? dto.Status : "Pending",
                    Category = dto.Category ?? string.Empty,
                    AssignedUserId = userId,
                    SubTasks = (dto.SubTasks ?? Enumerable.Empty<SubTaskDto>()).Select(st => new SubTask
                    {
                        Title = st.Title,
                        IsCompleted = st.IsCompleted
                    }).ToList()
                })
                .ToList();

            if (validTasks.Count > 0)
            {
                await _context.Tasks.AddRangeAsync(validTasks);
                await _context.SaveChangesAsync();
            }

            _logger.LogInformation("User ID {UserId} imported {Count} tasks.", userId, validTasks.Count);
            return validTasks.Count;
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
                IsAdminAssigned = task.IsAdminAssigned,
                IsDeleted = task.IsDeleted,
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
