using System.CommandLine;
using Memodex.Cli;

Command dbCommand = new Command("db");
dbCommand.AddCommand(DbSetCommandFactory.Create());
dbCommand.AddCommand(DbShowCommandFactory.Create());
dbCommand.AddCommand(DbWipeCommandFactory.Create());

Command categoryCommand = new Command("category");
categoryCommand.AddCommand(CategoryAddCommandFactory.Create());
categoryCommand.AddCommand(CategoryListCommandFactory.Create());

Command deckCommand = new Command("deck");
deckCommand.AddCommand(DeckAddCommandFactory.Create());
deckCommand.AddCommand(DeckListCommandFactory.Create());
deckCommand.AddCommand(DeckImportCommandFactory.Create());

Command flashcardCommand = new Command("flashcard");
flashcardCommand.AddCommand(FlashcardAddCommandFactory.Create());
flashcardCommand.AddCommand(FlashcardListCommandFactory.Create());

Command rootCommand = new RootCommand();
rootCommand.AddCommand(dbCommand);
rootCommand.AddCommand(categoryCommand);
rootCommand.AddCommand(deckCommand);
rootCommand.AddCommand(flashcardCommand);
await rootCommand.InvokeAsync(args);
