using System.CommandLine;

namespace Memodex.Cli;

public static class DbShowCommandFactory
{
    public static Command Create()
    {
        Command dbShowCommand = new Command("show");
        dbShowCommand.SetHandler(DbCommandHandler.HandleDbShow);

        return dbShowCommand;
    }
}