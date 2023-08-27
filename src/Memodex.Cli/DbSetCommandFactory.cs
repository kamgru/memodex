using System.CommandLine;

namespace Memodex.Cli;

public static class DbSetCommandFactory
{
    public static Command Create()
    {
        Option<string> hostOption = new(
            "--host",
            description: "The host to connect to.",
            getDefaultValue: () => "localhost"
        );

        Option<string> userOption = new(
            "--user",
            description: "The user to connect as.",
            getDefaultValue: () => "sa"
        );

        Option<string> passwordOption = new(
            "--password",
            description: "The password to use."
        );

        Option<string> databaseOption = new(
            "--database",
            description: "The database to connect to.",
            getDefaultValue: () => "MemodexDb"
        );
        
        Option<bool> initOption = new(
            "--init",
            description: "Initialize the database."
        );

        Command dbSetCommand = new("set")
        {
            hostOption,
            databaseOption,
            userOption,
            passwordOption,
            initOption
        };

        dbSetCommand.SetHandler(
            DbCommandHandler.HandleDbSet, 
            hostOption, 
            databaseOption, 
            userOption, 
            passwordOption, 
            initOption);

        return dbSetCommand;
    }
}