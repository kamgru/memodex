using System.CommandLine;
using Memodex.DataAccess;

namespace Memodex.Cli;

public static class DeckListCommandFactory
{
    public static Command Create()
    {
        Command command = new("list", "List all decks in the database");
        
        command.SetHandler(() =>
        {
            MemodexContext memodexContext = MemodexContextFactory.Create();
            foreach (Deck deck in memodexContext.Decks)
            {
                Console.WriteLine($"{deck.Id} - {deck.Name} - {deck.Description}");
            }
        });
        
        return command;
    }
}