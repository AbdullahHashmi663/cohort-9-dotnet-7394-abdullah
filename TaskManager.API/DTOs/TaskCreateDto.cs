using System.ComponentModel.DataAnnotations;

namespace TaskManager.API.DTOs
{
    public class TaskCreateDto
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateTime? DueDate { get; set; }

        [Required]
        public string Priority { get; set; } = "Medium"; // Low, Medium, High

        public string Status { get; set; } = "Pending"; // Pending, InProgress, Completed

        public string Category { get; set; } = string.Empty;
    }
}
