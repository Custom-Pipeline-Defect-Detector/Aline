namespace KanbanApi.Models;

public class StoryLabel
{
    public int StoryId { get; set; }
    public Story? Story { get; set; }

    public int LabelId { get; set; }
    public Label? Label { get; set; }
}
