using KanbanApi.Models;

namespace KanbanApi.DTOs;

public record StoryListDto(
    int Id,
    string Title,
    StoryStatus Status,
    StoryPriority Priority,
    string? AssigneeName,
    int CommentCount,
    int AttachmentCount);

public record StoryLabelDto(int Id, string Name, string Color);

public record StoryAttachmentDto(int Id, string FileName, string OriginalFileName, string ContentType, long Size, DateTime UploadedAt, string Url);

public record StoryTaskDto(int Id, string Title, string? Description, WorkTaskStatus Status, int AttachmentCount);

public record StoryCommentSummaryDto(int Id, string Content, DateTime CreatedAt, string AuthorName, int AttachmentCount);

public class StoryDetailDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public StoryStatus Status { get; set; }
    public StoryPriority Priority { get; set; }
    public string? AssigneeName { get; set; }
    public IEnumerable<StoryLabelDto> Labels { get; set; } = Enumerable.Empty<StoryLabelDto>();
    public IEnumerable<StoryAttachmentDto> Attachments { get; set; } = Enumerable.Empty<StoryAttachmentDto>();
    public IEnumerable<StoryTaskDto> Tasks { get; set; } = Enumerable.Empty<StoryTaskDto>();
    public IEnumerable<StoryCommentSummaryDto> Comments { get; set; } = Enumerable.Empty<StoryCommentSummaryDto>();
}

public class StoryCreateDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public StoryPriority Priority { get; set; } = StoryPriority.Medium;
    public StoryStatus Status { get; set; } = StoryStatus.Backlog;
    public int ProjectId { get; set; }
    public int? EpicId { get; set; }
    public int? AssigneeId { get; set; }
    public ICollection<int> LabelIds { get; set; } = new List<int>();
}

public class StoryUpdateDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public StoryPriority? Priority { get; set; }
    public StoryStatus? Status { get; set; }
    public int? EpicId { get; set; }
    public int? AssigneeId { get; set; }
    public ICollection<int>? LabelIds { get; set; }
}
