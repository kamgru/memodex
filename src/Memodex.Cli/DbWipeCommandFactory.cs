using System.CommandLine;
using Memodex.DataAccess;

namespace Memodex.Cli;

public class DbWipeCommandFactory
{
    public static Command Create()
    {
        Command command = new("wipe", "Wipe the database");
        
        command.SetHandler(DbCommandHandler.HandleDbWipe);
        
        return command;
    }
}