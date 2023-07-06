using System.CommandLine;
using Memodex.DataAccess;

namespace Memodex.Cli;

public static class FlashcardAddCommandFactory
{
    public static Command Create()
    {
        Option<string> questionOption = new(new[] { "--question", "-q" }, "The question of the flashcard to add")
        {
            IsRequired = true
        };
        
        Option<string> answerOption = new(new[] { "--answer", "-a" }, "The answer of the flashcard to add")
        {
            IsRequired = true
        };
        
        Option<int> deckIdOption = new(new[] { "--deck-id", "-d" }, "The deck id of the flashcard to add")
        {
            IsRequired = true
        };
        
        Command command = new("add", "Add a flashcard to the database")
        {
            questionOption,
            answerOption,
            deckIdOption
        };
        
        command.SetHandler((question, answer, deckId) =>
        {
           MemodexContext memodexContext = MemodexContextFactory.Create();
           memodexContext.Flashcards.Add(new Flashcard
           {
               Question = question,
               Answer = answer,
               DeckId = deckId
           });
           memodexContext.SaveChanges();
        }, questionOption, answerOption, deckIdOption);
        
        return command;
    }
}