using System.Linq;
using KanbanApi.Data;
using KanbanApi.DTOs;
using KanbanApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KanbanApi.Controllers;

[ApiController]
[Route("api/stories/{storyId:int}/[controller]")]
public class CommentsController : ControllerBase
{
    private readonly AppDbContext _context;

    public CommentsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CommentDto>>> GetComments(int storyId)
    {
        if (!await _context.Stories.AnyAsync(s => s.Id == storyId))
        {
            return NotFound();
        }

        var comments = await _context.Comments
            .AsNoTracking()
            .Where(c => c.StoryId == storyId)
            .Include(c => c.Author)
            .Include(c => c.Attachments)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new CommentDto(
                c.Id,
                c.Content,
                c.CreatedAt,
                c.Author != null ? c.Author.Name : string.Empty,
                c.Attachments.Select(a => new CommentAttachmentDto(
                    a.Id,
                    a.FileName,
                    a.OriginalFileName,
                    a.ContentType,
                    a.Size,
                    a.UploadedAt,
                    AttachmentUrlHelper.BuildAttachmentUrl(a))).ToList()))
            .ToListAsync();

        return Ok(comments);
    }

    [HttpPost]
    public async Task<ActionResult<CommentDto>> CreateComment(int storyId, [FromBody] CommentCreateDto dto)
    {
        var story = await _context.Stories.FindAsync(storyId);
        if (story == null)
        {
            return NotFound();
        }

        var userExists = await _context.Users.AnyAsync(u => u.Id == dto.AuthorId);
        if (!userExists)
        {
            return BadRequest($"User {dto.AuthorId} does not exist.");
        }

        var comment = new Comment
        {
            StoryId = storyId,
            AuthorId = dto.AuthorId,
            Content = dto.Content,
            CreatedAt = DateTime.UtcNow
        };

        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        if (dto.AttachmentIds.Any())
        {
            var attachments = await _context.Attachments
                .Where(a => dto.AttachmentIds.Contains(a.Id))
                .ToListAsync();

            foreach (var attachment in attachments)
            {
                if (attachment.CommentId.HasValue && attachment.CommentId != comment.Id)
                {
                    return BadRequest($"Attachment {attachment.Id} is already linked to another comment.");
                }

                if (attachment.StoryId.HasValue && attachment.StoryId != storyId)
                {
                    return BadRequest($"Attachment {attachment.Id} is linked to a different story.");
                }

                if (comment.TaskId.HasValue && attachment.TaskId.HasValue && attachment.TaskId != comment.TaskId)
                {
                    return BadRequest($"Attachment {attachment.Id} is linked to a different task.");
                }

                attachment.CommentId = comment.Id;
                attachment.StoryId ??= storyId;
            }

            await _context.SaveChangesAsync();
        }

        var created = await _context.Comments
            .AsNoTracking()
            .Include(c => c.Author)
            .Include(c => c.Attachments)
            .FirstAsync(c => c.Id == comment.Id);

        var dtoResult = new CommentDto(
            created.Id,
            created.Content,
            created.CreatedAt,
            created.Author?.Name ?? string.Empty,
            created.Attachments.Select(a => new CommentAttachmentDto(
                a.Id,
                a.FileName,
                a.OriginalFileName,
                a.ContentType,
                a.Size,
                a.UploadedAt,
                AttachmentUrlHelper.BuildAttachmentUrl(a))).ToList());

        return CreatedAtAction(nameof(GetComments), new { storyId }, dtoResult);
    }
}
