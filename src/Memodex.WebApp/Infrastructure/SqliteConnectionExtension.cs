using Microsoft.Data.Sqlite;

namespace Memodex.WebApp.Infrastructure;

public static class SqliteConnectionExtension
{
    public static SqliteCommand CreateCommand(
        this SqliteConnection connection,
        string commandText)
    {
       SqliteCommand command = connection.CreateCommand();
       command.CommandText = commandText;
       return command;
    }
}
