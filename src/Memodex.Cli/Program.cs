using System.CommandLine;
using Memodex.Cli;

Command dbCommand = new Command("db");
dbCommand.AddCommand(DbSetCommandFactory.Create());
dbCommand.AddCommand(DbShowCommandFactory.Create());

Command rootCommand = new RootCommand();
rootCommand.AddCommand(dbCommand);
await rootCommand.InvokeAsync(args);
