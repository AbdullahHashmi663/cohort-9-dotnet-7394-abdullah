namespace TaskManager.API.DTOs
{
    public class DashboardDto
    {
        public int PendingCount { get; set; }
        public int InProgressCount { get; set; }
        public int CompletedCount { get; set; }
        public int TotalCount { get; set; }

        public int TotalTasks { get => TotalCount; set => TotalCount = value; }
        public int PendingTasks { get => PendingCount; set => PendingCount = value; }
        public int InProgressTasks { get => InProgressCount; set => InProgressCount = value; }
        public int CompletedTasks { get => CompletedCount; set => CompletedCount = value; }

        public int HighPriorityTasks { get; set; }
        public int MediumPriorityTasks { get; set; }
        public int LowPriorityTasks { get; set; }
        public List<TaskResponseDto> RecentTasks { get; set; } = new();
    }
}
