using System.CommandLine;
using Memodex.DataAccess;

namespace Memodex.Cli;

public static class DeckAddCommandFactory
{
    public static Command Create()
    {
        Option<string> nameOption = new Option<string>(
            new [] {"--name", "-n"},
            description: "The name of the deck to add"
        )
        {
            IsRequired = true
        };
        
        Option<string> descriptionOption = new Option<string>(
            new [] {"--description", "-d"},
            description: "The description of the deck to add"
        )
        {
            IsRequired = true
        };
        
        Option<int> categoryIdOption = new Option<int>(
            new [] {"--category-id", "-c"},
            description: "The category id of the deck to add"
        )
        {
            IsRequired = true
        };
        
        Command command = new("add", "Add a deck to the database")
        {
            nameOption,
            descriptionOption,
            categoryIdOption
        };
        
        command.SetHandler((name, description, categoryId) =>
        {
            MemodexContext memodexContext = MemodexContextFactory.Create();
            memodexContext.Decks.Add(new Deck
            {
                Name = name,
                Description = description,
                CategoryId = categoryId,
                Flashcards = new List<Flashcard>()
            });
            
            memodexContext.SaveChanges();
        }, nameOption, descriptionOption, categoryIdOption);

        return command;
    }
}