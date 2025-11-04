namespace KanbanApi.Models;

public class Label
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#000000";

    public ICollection<StoryLabel> StoryLinks { get; set; } = new List<StoryLabel>();
}
