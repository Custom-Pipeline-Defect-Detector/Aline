namespace KanbanApi.Models;

public enum UserRole
{
    Admin,
    Boss,
    Worker
}

public enum StoryStatus
{
    Backlog,
    InProgress,
    Review,
    Done
}

public enum StoryPriority
{
    Low,
    Medium,
    High,
    Critical
}

public enum WorkTaskStatus
{
    Todo,
    Doing,
    Done
}
