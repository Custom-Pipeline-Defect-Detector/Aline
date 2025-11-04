namespace KanbanApi.Models;

public class Epic
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public int ProjectId { get; set; }
    public Project? Project { get; set; }

    public ICollection<Story> Stories { get; set; } = new List<Story>();
}
