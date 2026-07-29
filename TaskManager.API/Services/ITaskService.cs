using TaskManager.API.DTOs;

namespace TaskManager.API.Services
{
    public interface ITaskService
    {
        Task<IEnumerable<TaskResponseDto>> GetTasksAsync(int userId, string userRole);
        Task<TaskResponseDto> GetTaskByIdAsync(int taskId, int userId, string userRole);
        Task<TaskResponseDto> CreateTaskAsync(TaskCreateDto dto, int userId, string userRole = "User");
        Task<TaskResponseDto> UpdateTaskAsync(int taskId, TaskUpdateDto dto, int userId, string userRole);
        Task<string> DeleteTaskAsync(int taskId, int userId, string userRole);
        Task<DashboardDto> GetDashboardAsync(int userId, string userRole);
        Task<byte[]> ExportTasksAsync(int userId, string userRole);
        Task<int> ImportTasksAsync(IEnumerable<TaskCreateDto> dtos, int userId);
    }
}
