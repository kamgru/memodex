namespace Memodex.WebApp.Data;

public class Category
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int ItemCount { get; set; }
    public required ICollection<Flashcard> Flashcards { get; set; }
}