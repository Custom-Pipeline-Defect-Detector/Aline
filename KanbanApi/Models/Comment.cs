namespace KanbanApi.Models;

public class Comment
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int AuthorId { get; set; }
    public User? Author { get; set; }

    public int? StoryId { get; set; }
    public Story? Story { get; set; }

    public int? TaskId { get; set; }
    public WorkTask? Task { get; set; }

    public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
}
