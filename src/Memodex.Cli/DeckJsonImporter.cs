using System.Text.Json;
using Memodex.DataAccess;

namespace Memodex.Cli;

public record DeckData(string Name, string Description, List<FlashcardData> Flashcards);

public record FlashcardData(string Question, string Answer);

public static class DeckJsonImporter
{
    public static void Import(string filename, int categoryId)
    {
        string jsonData = File.ReadAllText(filename);
        DeckData deckData = JsonSerializer.Deserialize<DeckData>(jsonData, new JsonSerializerOptions
                            {
                                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                            })
                            ?? throw new InvalidOperationException("Unable to deserialize deck data");
        Deck deck = new()
        {
            CategoryId = categoryId,
            Name = deckData.Name,
            Description = deckData.Description,
            Flashcards = deckData.Flashcards
                .Select(item => new Flashcard
                {
                    Answer = item.Answer,
                    Question = item.Question
                })
                .ToList()
        };
        MemodexContext memodexContext = MemodexContextFactory.Create();
        memodexContext.Decks.Add(deck);
        memodexContext.SaveChanges();
    }

    public static void ImportMany(string directory, int categoryId)
    {
        string[] files = Directory.GetFiles(directory, "*.json");
        foreach (string file in files)
        {
            Import(file, categoryId);
        }
    }
}