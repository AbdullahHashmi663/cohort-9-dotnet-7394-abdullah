namespace TaskManager.API.Models
{
    public class SubTask
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public bool IsCompleted { get; set; } = false;

        public int TaskItemId { get; set; }
        public TaskItem? TaskItem { get; set; }
    }
}
