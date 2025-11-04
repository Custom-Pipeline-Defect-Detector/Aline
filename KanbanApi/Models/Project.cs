namespace KanbanApi.Models;

public class Project
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public int? TeamId { get; set; }
    public Team? Team { get; set; }

    public ICollection<Epic> Epics { get; set; } = new List<Epic>();
    public ICollection<Story> Stories { get; set; } = new List<Story>();
    public ICollection<Sprint> Sprints { get; set; } = new List<Sprint>();
}
