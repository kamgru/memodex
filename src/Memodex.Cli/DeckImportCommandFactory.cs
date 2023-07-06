using System.CommandLine;
using Memodex.DataAccess;

namespace Memodex.Cli;

public static class DeckImportCommandFactory
{
    public static Command Create()
    {
        Option<string> pathOption = new Option<string>(
            new[] { "--path", "-p" },
            description: "The path of the deck to import"
        )
        {
            IsRequired = true
        };

        Option<string> nameOption = new Option<string>(
            new[] { "--name", "-n" },
            description: "The name of the deck to import"
        )
        {
            IsRequired = true
        };

        Option<int> categoryIdOption = new Option<int>(
            new[] { "--category-id", "-c" },
            description: "The category id of the deck to import"
        )
        {
            IsRequired = true
        };

        Command command = new("import", "Import a deck to the database")
        {
            pathOption,
            nameOption,
            categoryIdOption
        };

        command.SetHandler((name, categoryId, filename) =>
        {
            MemodexContext memodexContext = MemodexContextFactory.Create();
            Deck deck = new()
            {
                Name = name,
                CategoryId = categoryId,
                Flashcards = new List<Flashcard>()
            };

            using TextReader textReader = File.OpenText(filename);
            string? line;
            while ((line = textReader.ReadLine()) != null)
            {
                string[] parts = line.Split('|');
                string question = parts[0];
                string answer = parts[1];
                deck.Flashcards.Add(new Flashcard
                {
                    Question = question,
                    Answer = answer
                });
            }

            memodexContext.Decks.Add(deck);
            memodexContext.SaveChanges();
        }, nameOption, categoryIdOption, pathOption);
        return command;
    }
}