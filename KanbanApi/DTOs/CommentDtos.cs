namespace KanbanApi.DTOs;

public record CommentAttachmentDto(int Id, string FileName, string OriginalFileName, string ContentType, long Size, DateTime UploadedAt, string Url);

public record CommentDto(int Id, string Content, DateTime CreatedAt, string AuthorName, IEnumerable<CommentAttachmentDto> Attachments);

public class CommentCreateDto
{
    public string Content { get; set; } = string.Empty;
    public int AuthorId { get; set; }
    public ICollection<int> AttachmentIds { get; set; } = new List<int>();
}
