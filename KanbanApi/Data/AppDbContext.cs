using KanbanApi.Models;
using Microsoft.EntityFrameworkCore;

namespace KanbanApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Epic> Epics => Set<Epic>();
    public DbSet<Story> Stories => Set<Story>();
    public DbSet<WorkTask> WorkTasks => Set<WorkTask>();
    public DbSet<Sprint> Sprints => Set<Sprint>();
    public DbSet<Label> Labels => Set<Label>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Attachment> Attachments => Set<Attachment>();
    public DbSet<StoryLabel> StoryLabels => Set<StoryLabel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasOne(u => u.Team)
            .WithMany(t => t.Workers)
            .HasForeignKey(u => u.TeamId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Team>()
            .HasOne(t => t.Boss)
            .WithMany()
            .HasForeignKey(t => t.BossId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Project>()
            .HasOne(p => p.Team)
            .WithMany(t => t.Projects)
            .HasForeignKey(p => p.TeamId);

        modelBuilder.Entity<Epic>()
            .HasOne(e => e.Project)
            .WithMany(p => p.Epics)
            .HasForeignKey(e => e.ProjectId);

        modelBuilder.Entity<Story>()
            .HasOne(s => s.Project)
            .WithMany(p => p.Stories)
            .HasForeignKey(s => s.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Story>()
            .HasOne(s => s.Epic)
            .WithMany(e => e.Stories)
            .HasForeignKey(s => s.EpicId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Story>()
            .HasOne(s => s.Assignee)
            .WithMany()
            .HasForeignKey(s => s.AssigneeId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<StoryLabel>()
            .HasKey(sl => new { sl.StoryId, sl.LabelId });

        modelBuilder.Entity<StoryLabel>()
            .HasOne(sl => sl.Story)
            .WithMany(s => s.LabelLinks)
            .HasForeignKey(sl => sl.StoryId);

        modelBuilder.Entity<StoryLabel>()
            .HasOne(sl => sl.Label)
            .WithMany(l => l.StoryLinks)
            .HasForeignKey(sl => sl.LabelId);

        modelBuilder.Entity<WorkTask>()
            .HasOne(t => t.Story)
            .WithMany(s => s.Tasks)
            .HasForeignKey(t => t.StoryId);

        modelBuilder.Entity<Comment>()
            .HasOne(c => c.Story)
            .WithMany(s => s.Comments)
            .HasForeignKey(c => c.StoryId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Comment>()
            .HasOne(c => c.Task)
            .WithMany(t => t.Comments)
            .HasForeignKey(c => c.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Comment>()
            .HasOne(c => c.Author)
            .WithMany(u => u.Comments)
            .HasForeignKey(c => c.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Attachment>()
            .HasOne(a => a.Story)
            .WithMany(s => s.Attachments)
            .HasForeignKey(a => a.StoryId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Attachment>()
            .HasOne(a => a.Task)
            .WithMany(t => t.Attachments)
            .HasForeignKey(a => a.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Attachment>()
            .HasOne(a => a.Comment)
            .WithMany(c => c.Attachments)
            .HasForeignKey(a => a.CommentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Sprint>()
            .HasOne(s => s.Project)
            .WithMany(p => p.Sprints)
            .HasForeignKey(s => s.ProjectId);
    }
}
