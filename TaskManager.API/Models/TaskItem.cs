namespace TaskManager.API.Models
{
    public class TaskItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
        public string Priority { get; set; } = "Medium"; 
        public string Status { get; set; } = "Pending"; 
        public string Category { get; set; } = string.Empty;
        
        // Soft delete flag
        public bool IsDeleted { get; set; } = false;

        public int AssignedUserId { get; set; }
        public User? AssignedUser { get; set; }
    }
}