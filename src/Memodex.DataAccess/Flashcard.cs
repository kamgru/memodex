namespace Memodex.DataAccess;

public class Flashcard
{
    public int Id { get; set; }
    public required string Question { get; set; }
    public required string Answer { get; set; }
    public int DeckId { get; set; }
    public Deck Deck { get; set; } = null!;
}