namespace KanbanApi.Models;

public class Attachment
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long Size { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public string RelativePath { get; set; } = string.Empty;

    public int? StoryId { get; set; }
    public Story? Story { get; set; }

    public int? TaskId { get; set; }
    public WorkTask? Task { get; set; }

    public int? CommentId { get; set; }
    public Comment? Comment { get; set; }
}
