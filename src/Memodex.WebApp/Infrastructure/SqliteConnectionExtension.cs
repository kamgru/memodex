using Microsoft.Data.Sqlite;

namespace Memodex.WebApp.Infrastructure;

public static class SqliteConnectionExtension
{
    public static SqliteCommand CreateCommand(
        this SqliteConnection connection,
        string commandText,
        IEnumerable<SqliteParameter>? parameters = null)
    {
        SqliteCommand command = connection.CreateCommand();
        command.CommandText = commandText;

        if (parameters != null)
        {
            command.Parameters.AddRange(parameters);
        }

        return command;
    }

}
