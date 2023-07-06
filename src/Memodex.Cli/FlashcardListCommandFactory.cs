using System.CommandLine;
using Memodex.DataAccess;

namespace Memodex.Cli;

public static class FlashcardListCommandFactory
{
    public static Command Create()
    {
        Command command = new("list", "List all flashcards in the database");
        
        command.SetHandler(() =>
        {
            MemodexContext memodexContext = MemodexContextFactory.Create();
            foreach (Flashcard flashcard in memodexContext.Flashcards)
            {
                Console.WriteLine($"{flashcard.Id} - {flashcard.Question} - {flashcard.Answer}");
            }
        });

        return command;
    }
}