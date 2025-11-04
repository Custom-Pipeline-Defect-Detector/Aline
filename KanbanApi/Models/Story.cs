namespace KanbanApi.Models;

public class Story
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public StoryStatus Status { get; set; } = StoryStatus.Backlog;
    public StoryPriority Priority { get; set; } = StoryPriority.Medium;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public int ProjectId { get; set; }
    public Project? Project { get; set; }

    public int? EpicId { get; set; }
    public Epic? Epic { get; set; }

    public int? AssigneeId { get; set; }
    public User? Assignee { get; set; }

    public ICollection<StoryLabel> LabelLinks { get; set; } = new List<StoryLabel>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
    public ICollection<WorkTask> Tasks { get; set; } = new List<WorkTask>();
}
