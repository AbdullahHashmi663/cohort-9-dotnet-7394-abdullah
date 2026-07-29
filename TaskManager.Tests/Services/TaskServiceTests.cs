using Microsoft.Extensions.Logging;
using Moq;
using TaskManager.API.DTOs;
using TaskManager.API.Models;
using TaskManager.API.Services;
using TaskManager.Tests.Helpers;

namespace TaskManager.Tests.Services
{
    public class TaskServiceTests
    {
        private TaskService CreateService(string dbName, out API.Data.AppDbContext context)
        {
            context = TestDbContextFactory.Create(dbName);
            var logger = new Mock<ILogger<TaskService>>();
            return new TaskService(context, logger.Object);
        }

        private async Task<User> SeedUser(API.Data.AppDbContext context, string name = "Test User", string role = "User")
        {
            var user = new User
            {
                Name = name,
                Email = $"{name.Replace(" ", "").ToLower()}@test.com",
                PasswordHash = "hashed",
                Role = role
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();
            return user;
        }

        [Fact]
        public async Task CreateTaskAsync_CreatesAndReturnsTask()
        {
            // Arrange
            var service = CreateService(nameof(CreateTaskAsync_CreatesAndReturnsTask), out var context);
            var user = await SeedUser(context);
            var dto = new TaskCreateDto
            {
                Title = "Test Task",
                Description = "A test task",
                Priority = "High",
                Status = "Pending",
                Category = "Work"
            };

            // Act
            var result = await service.CreateTaskAsync(dto, user.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Task", result.Title);
            Assert.Equal("High", result.Priority);
            Assert.Equal(user.Id, result.AssignedUserId);
        }

        [Fact]
        public async Task GetTasksAsync_RegularUser_ReturnsOnlyOwnTasks()
        {
            // Arrange
            var service = CreateService(nameof(GetTasksAsync_RegularUser_ReturnsOnlyOwnTasks), out var context);
            var user1 = await SeedUser(context, "User One");
            var user2 = await SeedUser(context, "User Two");

            await service.CreateTaskAsync(new TaskCreateDto { Title = "Task 1", Priority = "Low" }, user1.Id);
            await service.CreateTaskAsync(new TaskCreateDto { Title = "Task 2", Priority = "Low" }, user2.Id);

            // Act
            var result = await service.GetTasksAsync(user1.Id, "User");

            // Assert
            Assert.Single(result);
            Assert.Equal("Task 1", result.First().Title);
        }

        [Fact]
        public async Task GetTasksAsync_Admin_ReturnsAllTasks()
        {
            // Arrange
            var service = CreateService(nameof(GetTasksAsync_Admin_ReturnsAllTasks), out var context);
            var user1 = await SeedUser(context, "User One");
            var admin = await SeedUser(context, "Admin User", "Admin");

            await service.CreateTaskAsync(new TaskCreateDto { Title = "Task 1", Priority = "Low" }, user1.Id);
            await service.CreateTaskAsync(new TaskCreateDto { Title = "Task 2", Priority = "Low" }, admin.Id);

            // Act
            var result = await service.GetTasksAsync(admin.Id, "Admin");

            // Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetTaskByIdAsync_WithValidId_ReturnsTask()
        {
            // Arrange
            var service = CreateService(nameof(GetTaskByIdAsync_WithValidId_ReturnsTask), out var context);
            var user = await SeedUser(context);
            var created = await service.CreateTaskAsync(new TaskCreateDto { Title = "Find Me", Priority = "Medium" }, user.Id);

            // Act
            var result = await service.GetTaskByIdAsync(created.Id, user.Id, "User");

            // Assert
            Assert.Equal("Find Me", result.Title);
        }

        [Fact]
        public async Task GetTaskByIdAsync_WithInvalidId_ThrowsKeyNotFoundException()
        {
            // Arrange
            var service = CreateService(nameof(GetTaskByIdAsync_WithInvalidId_ThrowsKeyNotFoundException), out var context);
            var user = await SeedUser(context);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetTaskByIdAsync(999, user.Id, "User"));
        }

        [Fact]
        public async Task UpdateTaskAsync_WithValidData_UpdatesTask()
        {
            // Arrange
            var service = CreateService(nameof(UpdateTaskAsync_WithValidData_UpdatesTask), out var context);
            var user = await SeedUser(context);
            var created = await service.CreateTaskAsync(new TaskCreateDto { Title = "Original", Priority = "Low" }, user.Id);

            var updateDto = new TaskUpdateDto
            {
                Title = "Updated",
                Description = "Updated description",
                Priority = "High",
                Status = "InProgress",
                Category = "Personal"
            };

            // Act
            var result = await service.UpdateTaskAsync(created.Id, updateDto, user.Id, "User");

            // Assert
            Assert.Equal("Updated", result.Title);
            Assert.Equal("High", result.Priority);
            Assert.Equal("InProgress", result.Status);
        }

        [Fact]
        public async Task DeleteTaskAsync_WithValidId_SoftDeletesTask()
        {
            // Arrange
            var service = CreateService(nameof(DeleteTaskAsync_WithValidId_SoftDeletesTask), out var context);
            var user = await SeedUser(context);
            var created = await service.CreateTaskAsync(new TaskCreateDto { Title = "Delete Me", Priority = "Low" }, user.Id);

            // Act
            var result = await service.DeleteTaskAsync(created.Id, user.Id, "User");

            // Assert
            Assert.Equal("Task deleted successfully.", result);

            // Verify the task is soft-deleted (query filter hides it)
            var tasks = await service.GetTasksAsync(user.Id, "User");
            Assert.Empty(tasks);
        }

        [Fact]
        public async Task GetDashboardAsync_ReturnsCorrectCounts()
        {
            // Arrange
            var service = CreateService(nameof(GetDashboardAsync_ReturnsCorrectCounts), out var context);
            var user = await SeedUser(context);

            await service.CreateTaskAsync(new TaskCreateDto { Title = "T1", Priority = "Low", Status = "Pending" }, user.Id);
            await service.CreateTaskAsync(new TaskCreateDto { Title = "T2", Priority = "Low", Status = "Pending" }, user.Id);
            await service.CreateTaskAsync(new TaskCreateDto { Title = "T3", Priority = "Low", Status = "InProgress" }, user.Id);
            await service.CreateTaskAsync(new TaskCreateDto { Title = "T4", Priority = "Low", Status = "Completed" }, user.Id);

            // Act
            var result = await service.GetDashboardAsync(user.Id, "User");

            // Assert
            Assert.Equal(2, result.PendingCount);
            Assert.Equal(1, result.InProgressCount);
            Assert.Equal(1, result.CompletedCount);
            Assert.Equal(4, result.TotalCount);
        }

        [Fact]
        public async Task DeleteTaskAsync_OtherUserTask_ThrowsKeyNotFoundException()
        {
            // Arrange
            var service = CreateService(nameof(DeleteTaskAsync_OtherUserTask_ThrowsKeyNotFoundException), out var context);
            var user1 = await SeedUser(context, "User One");
            var user2 = await SeedUser(context, "User Two");
            var created = await service.CreateTaskAsync(new TaskCreateDto { Title = "Private", Priority = "Low" }, user1.Id);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.DeleteTaskAsync(created.Id, user2.Id, "User"));
        }

        [Fact]
        public async Task ExportTasksAsync_ReturnsJsonBytes()
        {
            // Arrange
            var service = CreateService(nameof(ExportTasksAsync_ReturnsJsonBytes), out var context);
            var user = await SeedUser(context);
            await service.CreateTaskAsync(new TaskCreateDto { Title = "Export Me", Priority = "High" }, user.Id);

            // Act
            var bytes = await service.ExportTasksAsync(user.Id, "User");

            // Assert
            Assert.NotNull(bytes);
            Assert.NotEmpty(bytes);
            var json = System.Text.Encoding.UTF8.GetString(bytes);
            Assert.Contains("Export Me", json);
        }

        [Fact]
        public async Task ImportTasksAsync_AddsTasksToDatabase()
        {
            // Arrange
            var service = CreateService(nameof(ImportTasksAsync_AddsTasksToDatabase), out var context);
            var user = await SeedUser(context);

            var itemsToImport = new List<TaskCreateDto>
            {
                new TaskCreateDto { Title = "Imported 1", Priority = "Low", Status = "Pending" },
                new TaskCreateDto { Title = "Imported 2", Priority = "High", Status = "InProgress" }
            };

            // Act
            var count = await service.ImportTasksAsync(itemsToImport, user.Id);

            // Assert
            Assert.Equal(2, count);
            var tasks = await service.GetTasksAsync(user.Id, "User");
            Assert.Equal(2, tasks.Count());
        }
    }
}
