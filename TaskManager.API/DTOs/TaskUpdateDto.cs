using System.ComponentModel.DataAnnotations;

namespace TaskManager.API.DTOs
{
    public class TaskUpdateDto
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateTime? DueDate { get; set; }

        [Required]
        public string Priority { get; set; } = "Medium";

        [Required]
        public string Status { get; set; } = "Pending";

        public string Category { get; set; } = string.Empty;
    }
}
