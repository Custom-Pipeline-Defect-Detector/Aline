namespace KanbanApi.Models;

public class Team
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public int? BossId { get; set; }
    public User? Boss { get; set; }

    public ICollection<User> Workers { get; set; } = new List<User>();
    public ICollection<Project> Projects { get; set; } = new List<Project>();
}
