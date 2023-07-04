using System.CommandLine;
using System.CommandLine.Invocation;

namespace Memodex.Cli;

public static class DbSetCommandFactory
{
    public static Command Create()
    {
        Option<string> hostOption = new Option<string>(
            "--host",
            description: "The host to connect to.",
            getDefaultValue: () => "localhost"
        );

        Option<string> userOption = new Option<string>(
            "--user",
            description: "The user to connect as.",
            getDefaultValue: () => "sa"
        );

        Option<string> passwordOption = new Option<string>(
            "--password",
            description: "The password to use."
        );

        Option<string> databaseOption = new Option<string>(
            "--database",
            description: "The database to connect to.",
            getDefaultValue: () => "MemodexDb"
        );

        Command dbSetCommand = new Command("set")
        {
            hostOption,
            databaseOption,
            userOption,
            passwordOption
        };

        dbSetCommand.SetHandler(Handle, hostOption, databaseOption, userOption, passwordOption);

        return dbSetCommand;
    }

    private static void Handle(string host, string db, string user, string password)
    {
        string connectionString = $"Server={host};Database={db};User Id={user};Password={password};TrustServerCertificate=True";

        ConnectionStringManager.SetConnectionString(new ConnectionString(connectionString));
    }
}