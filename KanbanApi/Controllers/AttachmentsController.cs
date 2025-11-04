using System.IO;
using KanbanApi.Data;
using KanbanApi.DTOs;
using KanbanApi.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KanbanApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AttachmentsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public AttachmentsController(AppDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    [HttpPost]
    [RequestSizeLimit(104857600)]
    public async Task<ActionResult<AttachmentUploadResultDto>> UploadAttachment([FromForm] IFormFile file, [FromForm] int? storyId, [FromForm] int? taskId, [FromForm] int? commentId)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("A file must be provided.");
        }

        if (storyId.HasValue && !await _context.Stories.AnyAsync(s => s.Id == storyId))
        {
            return BadRequest($"Story {storyId} does not exist.");
        }

        WorkTask? task = null;
        if (taskId.HasValue)
        {
            task = await _context.WorkTasks
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task is null)
            {
                return BadRequest($"Task {taskId} does not exist.");
            }

            storyId ??= task.StoryId;
        }

        Comment? comment = null;
        if (commentId.HasValue)
        {
            comment = await _context.Comments
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == commentId);

            if (comment is null)
            {
                return BadRequest($"Comment {commentId} does not exist.");
            }

            storyId ??= comment.StoryId;
            taskId ??= comment.TaskId;
        }

        var uploadsFolder = Path.Combine(_environment.ContentRootPath, "uploads");
        Directory.CreateDirectory(uploadsFolder);

        var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        await using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await file.CopyToAsync(stream);
        }

        var attachment = new Attachment
        {
            FileName = uniqueFileName,
            OriginalFileName = file.FileName,
            ContentType = string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType,
            Size = file.Length,
            RelativePath = uniqueFileName,
            StoryId = storyId,
            TaskId = taskId,
            CommentId = commentId,
            UploadedAt = DateTime.UtcNow
        };

        _context.Attachments.Add(attachment);
        await _context.SaveChangesAsync();

        var dto = new AttachmentUploadResultDto
        {
            Id = attachment.Id,
            FileName = attachment.FileName,
            OriginalFileName = attachment.OriginalFileName,
            ContentType = attachment.ContentType,
            Size = attachment.Size,
            Url = AttachmentUrlHelper.BuildAttachmentUrl(attachment)
        };

        return CreatedAtAction(nameof(DownloadAttachment), new { id = attachment.Id }, dto);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> DownloadAttachment(int id)
    {
        var attachment = await _context.Attachments.FindAsync(id);
        if (attachment == null)
        {
            return NotFound();
        }

        var uploadsFolder = Path.Combine(_environment.ContentRootPath, "uploads");
        var fileName = Path.GetFileName(attachment.RelativePath);
        var filePath = Path.Combine(uploadsFolder, fileName);

        if (!System.IO.File.Exists(filePath))
        {
            return NotFound();
        }

        var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return File(stream, attachment.ContentType, attachment.OriginalFileName);
    }

    [HttpGet("{id:int}/metadata")]
    public async Task<ActionResult<AttachmentUploadResultDto>> GetMetadata(int id)
    {
        var attachment = await _context.Attachments.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
        if (attachment == null)
        {
            return NotFound();
        }

        var dto = new AttachmentUploadResultDto
        {
            Id = attachment.Id,
            FileName = attachment.FileName,
            OriginalFileName = attachment.OriginalFileName,
            ContentType = attachment.ContentType,
            Size = attachment.Size,
            Url = AttachmentUrlHelper.BuildAttachmentUrl(attachment)
        };

        return Ok(dto);
    }
}
