using System.CommandLine;

namespace Memodex.Cli;

public static class DbShowCommandFactory
{
    public static Command Create()
    {
        Command dbShowCommand = new Command("show");
        dbShowCommand.SetHandler(Handle);

        return dbShowCommand;
    }

    private static void Handle()
    {
        string connectionString = ConnectionStringManager.GetConnectionString().Value;
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            Console.WriteLine("No connection string found. Please set one using the 'db set' command.");
        }
    }
}