namespace TaskManager.API.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = "User"; // Roles: "Admin", "User"

        // Navigation property: A user can have many tasks
        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    }
}