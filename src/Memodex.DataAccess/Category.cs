namespace Memodex.DataAccess;

public class Category
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int ItemCount { get; set; }
    public required ICollection<Deck> Decks { get; set; }
}