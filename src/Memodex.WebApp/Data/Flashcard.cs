namespace Memodex.WebApp.Data;

public class Flashcard
{
    public int Id { get; set; }
    public required string Question { get; set; }
    public required string Answer { get; set; }
    public required Category Category { get; set; }
}