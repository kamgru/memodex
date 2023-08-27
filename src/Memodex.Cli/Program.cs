using System.CommandLine;
using Memodex.Cli;

Command dbCommand = new("db");
dbCommand.AddCommand(DbSetCommandFactory.Create());
dbCommand.AddCommand(DbShowCommandFactory.Create());
dbCommand.AddCommand(DbWipeCommandFactory.Create());
dbCommand.AddCommand(DbInitCommandFactory.Create());

Command categoryCommand = new("category");
categoryCommand.AddCommand(CategoryAddCommandFactory.Create(CategoryAddDefaultCommandFactory.Create()));
categoryCommand.AddCommand(CategoryListCommandFactory.Create());

Command deckCommand = new("deck");
deckCommand.AddCommand(DeckAddCommandFactory.Create());
deckCommand.AddCommand(DeckListCommandFactory.Create());
deckCommand.AddCommand(DeckImportCommandFactory.Create());

Command flashcardCommand = new("flashcard");
flashcardCommand.AddCommand(FlashcardAddCommandFactory.Create());
flashcardCommand.AddCommand(FlashcardListCommandFactory.Create());

Command avatarCommand = new("avatar");
avatarCommand.AddCommand(AvatarImportCommandFactory.Create());

Command profileCommand = new("profile");
profileCommand.AddCommand(ProfileCreateDefaultCommandFactory.Create());

Command initCommand = new("init");

Command rootCommand = new RootCommand();
rootCommand.AddCommand(dbCommand);
rootCommand.AddCommand(categoryCommand);
rootCommand.AddCommand(deckCommand);
rootCommand.AddCommand(flashcardCommand);
rootCommand.AddCommand(avatarCommand);
rootCommand.AddCommand(profileCommand);
rootCommand.AddCommand(initCommand);
await rootCommand.InvokeAsync(args);
