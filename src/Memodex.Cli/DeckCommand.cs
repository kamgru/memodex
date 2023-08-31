namespace Memodex.Cli;

public class DeckCommand : Command
{
    public DeckCommand()
        : base("deck", "Manage decks")
    {
        AddCommand(new DeckListCommand());
        AddCommand(new DeckImportCommand());
    }
}

public class DeckListCommand : Command
{
    public DeckListCommand()
        : base("list", "List all decks")
    {
        this.SetHandler(Handle);
    }

    private void Handle(
        InvocationContext invocationContext)
    {
        MemodexContext memodexContext = invocationContext.GetRequiredService<MemodexContextFactory>()
            .Create();

        List<Deck> decks = memodexContext.Decks
            .Include(deck => deck.Category)
            .OrderBy(deck => deck.CategoryId)
            .ToList();

        Console.WriteLine("{0,-10} {1,-15} {2, -15} {3,-30}", "Id", "Name", "Category", "Description");
        Console.WriteLine("{0,-10} {1,-15} {2, -15} {3,-30}", "--", "----", "--------", "-----------");
        foreach (Deck deck in decks)
        {
            Console.WriteLine("{0,-10} {1,-15} {2, -15} {3,-30}", deck.Id, deck.Name, deck.Category.Name,
                deck.Description);
        }
    }
}

public class DeckImportCommand : Command
{
    private record FlashcardData
    {
        public required string Question { get; set; }
        public required string Answer { get; set; }
    }

    private record DeckData
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required List<FlashcardData> Flashcards { get; set; } = new();
    }

    private readonly Option<string> _categoryNameOption = new("--category-name")
    {
        Description = "The name of the category to add the deck to",
        IsRequired = true
    };

    private readonly Option<string> _filenameOption = new("--filename")
    {
        Description = "The filename of the deck to import",
        IsRequired = true
    };

    public DeckImportCommand()
        : base("import", "Import a deck from a file")
    {
        AddOption(_categoryNameOption);
        AddOption(_filenameOption);
        this.SetHandler(Handle);
    }

    private void Handle(
        InvocationContext invocationContext)
    {
        string categoryName = invocationContext.GetOptionValue(_categoryNameOption);
        string filename = invocationContext.GetOptionValue(_filenameOption);

        MemodexContext memodexContext = invocationContext.GetRequiredService<MemodexContextFactory>()
            .Create();

        Category category = memodexContext.Categories
                                .FirstOrDefault(item => item.Name == categoryName)
                            ?? throw new InvalidOperationException();

        string jsonData = File.ReadAllText(filename);
        DeckData deckData = JsonSerializer.Deserialize<DeckData>(jsonData, new JsonSerializerOptions()
                            {
                                PropertyNameCaseInsensitive = true
                            })
                            ?? throw new InvalidOperationException("Invalid deck data");

        Deck deck = new()
        {
            CategoryId = category.Id,
            Name = deckData.Name,
            Description = deckData.Description,
            Flashcards = deckData.Flashcards.Select(flashcardData => new Flashcard
                {
                    Question = flashcardData.Question,
                    Answer = flashcardData.Answer
                })
                .ToList()
        };

        memodexContext.Decks.Add(deck);
        memodexContext.SaveChanges();

        Console.WriteLine("{0,-5} {1, 15} {2, 15}", "Id", "Name", "Category");
        Console.WriteLine("{0,-5} {1, 15} {2, 15}", "--", "----", "--------");
        Console.WriteLine("{0,-5} {1, 15} {2, 15}", deck.Id, deck.Name, category.Name);
    }
}