using System.Linq;
using KanbanApi.Data;
using KanbanApi.DTOs;
using KanbanApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KanbanApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StoriesController : ControllerBase
{
    private readonly AppDbContext _context;

    public StoriesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<StoryListDto>>> GetStories()
    {
        var stories = await _context.Stories
            .AsNoTracking()
            .Include(s => s.Assignee)
            .Include(s => s.Comments)
                .ThenInclude(c => c.Attachments)
            .Include(s => s.Attachments)
            .Include(s => s.Tasks)
                .ThenInclude(t => t.Attachments)
            .ToListAsync();

        var results = stories
            .Select(s =>
            {
                var attachmentIds = s.Attachments.Select(a => a.Id)
                    .Concat(s.Tasks.SelectMany(t => t.Attachments).Select(a => a.Id))
                    .Concat(s.Comments.SelectMany(c => c.Attachments).Select(a => a.Id))
                    .Distinct()
                    .Count();

                return new StoryListDto(
                    s.Id,
                    s.Title,
                    s.Status,
                    s.Priority,
                    s.Assignee?.Name,
                    s.Comments.Count,
                    attachmentIds);
            })
            .ToList();

        return Ok(results);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<StoryDetailDto>> GetStory(int id)
    {
        var story = await BuildStoryDetailAsync(id);
        if (story is null)
        {
            return NotFound();
        }

        return Ok(story);
    }

    [HttpPost]
    public async Task<ActionResult<StoryDetailDto>> CreateStory([FromBody] StoryCreateDto dto)
    {
        if (!await _context.Projects.AnyAsync(p => p.Id == dto.ProjectId))
        {
            return BadRequest($"Project {dto.ProjectId} does not exist.");
        }

        if (dto.EpicId.HasValue && !await _context.Epics.AnyAsync(e => e.Id == dto.EpicId))
        {
            return BadRequest($"Epic {dto.EpicId} does not exist.");
        }

        if (dto.AssigneeId.HasValue && !await _context.Users.AnyAsync(u => u.Id == dto.AssigneeId))
        {
            return BadRequest($"User {dto.AssigneeId} does not exist.");
        }

        var story = new Story
        {
            Title = dto.Title,
            Description = dto.Description,
            Priority = dto.Priority,
            Status = dto.Status,
            ProjectId = dto.ProjectId,
            EpicId = dto.EpicId,
            AssigneeId = dto.AssigneeId,
            CreatedAt = DateTime.UtcNow
        };

        await UpdateStoryLabelsAsync(story, dto.LabelIds);

        _context.Stories.Add(story);
        await _context.SaveChangesAsync();

        var detail = await BuildStoryDetailAsync(story.Id);
        return CreatedAtAction(nameof(GetStory), new { id = story.Id }, detail);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<StoryDetailDto>> UpdateStory(int id, [FromBody] StoryUpdateDto dto)
    {
        var story = await _context.Stories
            .Include(s => s.LabelLinks)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (story is null)
        {
            return NotFound();
        }

        if (dto.Title != null)
        {
            story.Title = dto.Title;
        }

        if (dto.Description != null)
        {
            story.Description = dto.Description;
        }

        if (dto.Priority.HasValue)
        {
            story.Priority = dto.Priority.Value;
        }

        if (dto.Status.HasValue)
        {
            story.Status = dto.Status.Value;
        }

        if (dto.EpicId.HasValue)
        {
            if (dto.EpicId.Value != 0 && !await _context.Epics.AnyAsync(e => e.Id == dto.EpicId.Value))
            {
                return BadRequest($"Epic {dto.EpicId} does not exist.");
            }

            story.EpicId = dto.EpicId.Value == 0 ? null : dto.EpicId;
        }

        if (dto.AssigneeId.HasValue)
        {
            if (dto.AssigneeId.Value != 0 && !await _context.Users.AnyAsync(u => u.Id == dto.AssigneeId.Value))
            {
                return BadRequest($"User {dto.AssigneeId} does not exist.");
            }

            story.AssigneeId = dto.AssigneeId.Value == 0 ? null : dto.AssigneeId;
        }

        if (dto.LabelIds != null)
        {
            await UpdateStoryLabelsAsync(story, dto.LabelIds);
        }

        story.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var detail = await BuildStoryDetailAsync(id);
        return Ok(detail);
    }

    private async Task UpdateStoryLabelsAsync(Story story, IEnumerable<int> labelIds)
    {
        var existing = story.LabelLinks.ToList();
        foreach (var link in existing)
        {
            _context.StoryLabels.Remove(link);
        }

        story.LabelLinks.Clear();

        if (!labelIds.Any())
        {
            return;
        }

        var labels = await _context.Labels
            .Where(l => labelIds.Contains(l.Id))
            .ToListAsync();

        foreach (var label in labels)
        {
            story.LabelLinks.Add(new StoryLabel
            {
                Story = story,
                Label = label
            });
        }
    }

    private async Task<StoryDetailDto?> BuildStoryDetailAsync(int id)
    {
        var story = await _context.Stories
            .AsNoTracking()
            .Include(s => s.Assignee)
            .Include(s => s.LabelLinks)
                .ThenInclude(sl => sl.Label)
            .Include(s => s.Attachments)
            .Include(s => s.Tasks)
                .ThenInclude(t => t.Attachments)
            .Include(s => s.Comments)
                .ThenInclude(c => c.Author)
            .Include(s => s.Comments)
                .ThenInclude(c => c.Attachments)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (story == null)
        {
            return null;
        }

        return new StoryDetailDto
        {
            Id = story.Id,
            Title = story.Title,
            Description = story.Description,
            Status = story.Status,
            Priority = story.Priority,
            AssigneeName = story.Assignee?.Name,
            Labels = story.LabelLinks
                .Where(l => l.Label != null)
                .Select(l => new StoryLabelDto(l.Label!.Id, l.Label.Name, l.Label.Color))
                .ToList(),
            Attachments = story.Attachments
                .Where(a => a.CommentId == null && a.TaskId == null)
                .Select(a => new StoryAttachmentDto(
                    a.Id,
                    a.FileName,
                    a.OriginalFileName,
                    a.ContentType,
                    a.Size,
                    a.UploadedAt,
                    AttachmentUrlHelper.BuildAttachmentUrl(a)))
                .ToList(),
            Tasks = story.Tasks
                .Select(t => new StoryTaskDto(
                    t.Id,
                    t.Title,
                    t.Description,
                    t.Status,
                    t.Attachments.Count(a => a.CommentId == null)))
                .ToList(),
            Comments = story.Comments
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new StoryCommentSummaryDto(
                    c.Id,
                    c.Content,
                    c.CreatedAt,
                    c.Author?.Name ?? "",
                    c.Attachments.Count))
                .ToList()
        };
    }

}
