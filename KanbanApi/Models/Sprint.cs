namespace KanbanApi.Models;

public class Sprint
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public int ProjectId { get; set; }
    public Project? Project { get; set; }
}
