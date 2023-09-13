using Microsoft.Data.Sqlite;

namespace Memodex.WebApp.Infrastructure;

public static class SqliteConnectionFactory
{
    public static SqliteConnection Create(
        string databaseName,
        bool enableForeignKeys = false)
    {
        string connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = databaseName,
            ForeignKeys = enableForeignKeys
        }.ToString();

        return new SqliteConnection(connectionString);
    }
}