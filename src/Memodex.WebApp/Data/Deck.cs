namespace Memodex.WebApp.Data;

public class Deck
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int ItemCount { get; set; }
    public ICollection<Flashcard> Flashcards { get; set; } = new List<Flashcard>();
    public Category Category { get; set; }
}